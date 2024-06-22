using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JustAssets.TerrainUtility
{
    /// <summary>
    ///     Splits terrain in nxn pieces.
    /// </summary>
    public sealed class SplitTerrain : IHandler<Terrain>
    {
        private const int MINIMAL_HEIGHTMAP_RESOLUTION = 33;

        private const int MINIMAL_CONTROL_TEXTURE_RESOLUTION = 16;

        private const int MINIMAL_BASE_TEXTURE_RESOLUTION = 16;

        private const int MINIMAL_DETAIL_RESOLUTION = 8;

        private bool _doSplitTrees = true;

        private int _targetPiecesPerAxis = 2;

        private Dictionary<(TerrainLayer layerSource, Vector2 newTileOffset), TerrainLayer> _layerData = new Dictionary<(TerrainLayer layerSource, Vector2 newTileOffset), TerrainLayer>();
        private bool _deactivateSource;

        public Terrain[] Selection { get; set; }

        public bool IsValid(out string reason)
        {
            if (Selection == null)
            {
                reason = "Select some terrain.";
                return false;
            }

            var selectionCount = Selection.Count(x => x != null);
            if (selectionCount == 0)
            {
                reason =
                    $"Currently no source terrain is selected. Please select at least one terrain. Each terrain will be split into {_targetPiecesPerAxis} pieces.";
                return false;
            }

            if (!ValidLayers(Selection))
            {
                reason = "Your terrain uses missing terrain layers (textures). Please remove or fix them before proceeding.";
                return false;
            }

            reason = null;

            if (!ValidLayerTiling(Selection))
            {
                reason =
                    "Some of your terrain layers use a Tiling larger than 1 or not equal to 1/2^x. This will result in creating new terrain layers.";
            }

            if (!TerrainMath.IsPowerOf2(_targetPiecesPerAxis))
            {
                reason = reason == null ? "" : $"{reason}\n";
                reason +=
                    $"Slicing the terrain in {_targetPiecesPerAxis}² results in {_targetPiecesPerAxis * _targetPiecesPerAxis} pieces which is not a power of 2². This will result in an approximated result. Perfect slices are only possible with e.g. 2², 4², 8²... slices.";
            }

            return true;
        }
        
        public T Get<T>(int index)
        {
            return (T)this[index];
        }

        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _doSplitTrees;
                    case 1:
                        return _targetPiecesPerAxis;
                    case 2:
                        return _deactivateSource;
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _doSplitTrees = (bool) value;
                        break;
                    case 1:
                        _targetPiecesPerAxis = (int) value;
                        break;
                    case 2:
                        _deactivateSource = (bool) value;
                        break;
                }
            }
        }

        public void Execute() => Split();

        private bool ValidLayers(Terrain[] selection)
        {
            var layers = selection.Select(x => x.terrainData).SelectMany(x => x.terrainLayers).Where(x => x == null);

            return !layers.Any();
        }

        private bool ValidLayerTiling(Terrain[] selection)
        {
            var layers = selection
                .Select(x => x.terrainData)
                .SelectMany(x => x.terrainLayers)
                .Select(x => x.tileSize)
                .Distinct()
                .Where(x => IsTilable(x.x) || IsTilable(x.y));

            return !layers.Any();
        }

        private static bool IsTilable(float size)
        {
            var inverse = 1 / size;
            return size > 1 || inverse % 1f > 0.001f || !TerrainMath.IsPowerOf2(Mathf.RoundToInt(inverse));
        }

        public void Split()
        {
            if (!IsValid(out var reason))
                throw new Exception(reason);

            if (Selection.Length == 0)
                return;

            var piecesPerAxis = _targetPiecesPerAxis;

            for (var parentTerrainIndex = 0; parentTerrainIndex < Selection.Length; parentTerrainIndex++)
            {
                Terrain sourceTerrain = Selection[parentTerrainIndex];
                EditorUtility.DisplayProgressBar("Split terrain", "Process " + parentTerrainIndex, (float) parentTerrainIndex / Selection.Length);
                SplitTerrainTile(piecesPerAxis, sourceTerrain);
            }

            EditorUtility.ClearProgressBar();
        }

        private void CopyPrototypes(TerrainData sourceTerrainData, TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex)
        {
            targetTerrainData.terrainLayers = sourceTerrainData.terrainLayers
                .Select(x => GetOrCreateTerrainLayer(sourceTerrainData.size, piecesPerAxis, sliceIndex, x))
                .ToArray();
            
            targetTerrainData.detailPrototypes = sourceTerrainData.detailPrototypes;
            targetTerrainData.treePrototypes = sourceTerrainData.treePrototypes;
        }

        private TerrainLayer GetOrCreateTerrainLayer(Vector3 sourceSize, int piecesPerAxis, int sliceIndex, TerrainLayer layerSource)
        {
            var targetSize = new Vector2(sourceSize.x / piecesPerAxis, sourceSize.z / piecesPerAxis);
            var shift = ComputeShift(piecesPerAxis, sliceIndex, targetSize.y, targetSize.x);
            Vector2 layerSize = layerSource.tileSize;

            var newTileOffset = new Vector2(
                (layerSource.tileOffset.x + shift.y * piecesPerAxis + layerSize.x) % layerSize.x,
                (layerSource.tileOffset.y + shift.x * piecesPerAxis + layerSize.y) % layerSize.y);

            var assetPath = AssetDatabase.GetAssetPath(layerSource);
            var isSourceLayerAnAsset = !string.IsNullOrEmpty(assetPath);

            if (!isSourceLayerAnAsset)
            {
                if (!AssetDatabase.IsValidFolder("Assets/TerrainLayers"))
                    AssetDatabase.CreateFolder("Assets", "TerrainLayers");

                var layerSourceName = Guid.NewGuid().ToString();
                assetPath = $"Assets/TerrainLayers/{layerSourceName}.terrainlayer";
                layerSource.name = layerSourceName;
                layerSource.SaveNewAsset(assetPath);

                layerSource = AssetDatabase.LoadAssetAtPath<TerrainLayer>(assetPath);
            }

            var originalLayer = (layerSource, Vector2.zero);
            if (!_layerData.ContainsKey(originalLayer))
                _layerData[originalLayer] = layerSource;

            if (!_layerData.TryGetValue((layerSource, newTileOffset), out var layer))
            {
                assetPath = TerrainExtensions.CreateLayerPath(assetPath, newTileOffset);

                var terrainLayer = layerSource.CloneTerrainLayer(layerSource.name, newTileOffset);

                terrainLayer.SaveNewAsset(assetPath);

                _layerData[(layerSource, newTileOffset)] = layer = terrainLayer;
            }

            return layer;
        }

        private void CopyTerrainProperties(Terrain sourceTerrain, Terrain targetTerrain, int piecesPerAxis)
        {
            targetTerrain.basemapDistance = sourceTerrain.basemapDistance;
            targetTerrain.shadowCastingMode = sourceTerrain.shadowCastingMode;
            targetTerrain.detailObjectDensity = sourceTerrain.detailObjectDensity;
            targetTerrain.detailObjectDistance = sourceTerrain.detailObjectDistance;
            targetTerrain.heightmapMaximumLOD = sourceTerrain.heightmapMaximumLOD;
            targetTerrain.heightmapPixelError = sourceTerrain.heightmapPixelError;
            targetTerrain.treeBillboardDistance = sourceTerrain.treeBillboardDistance;
            targetTerrain.treeCrossFadeLength = sourceTerrain.treeCrossFadeLength;
            targetTerrain.treeDistance = sourceTerrain.treeDistance;
            targetTerrain.treeMaximumFullLODCount = sourceTerrain.treeMaximumFullLODCount;
            targetTerrain.bakeLightProbesForTrees = sourceTerrain.bakeLightProbesForTrees;
            targetTerrain.drawInstanced = sourceTerrain.drawInstanced;
            targetTerrain.reflectionProbeUsage = sourceTerrain.reflectionProbeUsage;
            targetTerrain.realtimeLightmapScaleOffset = sourceTerrain.realtimeLightmapScaleOffset;
            targetTerrain.lightmapScaleOffset = sourceTerrain.lightmapScaleOffset;

            SetLightmapScale(sourceTerrain, targetTerrain, piecesPerAxis);

            targetTerrain.terrainData.wavingGrassAmount = sourceTerrain.terrainData.wavingGrassAmount;
            targetTerrain.terrainData.wavingGrassSpeed = sourceTerrain.terrainData.wavingGrassSpeed;
            targetTerrain.terrainData.wavingGrassStrength = sourceTerrain.terrainData.wavingGrassStrength;
            targetTerrain.terrainData.wavingGrassTint = sourceTerrain.terrainData.wavingGrassTint;
        }

        private void SetLightmapScale(Terrain sourceTerrain, Terrain targetTerrain, int piecesPerAxis)
        {
            var sos = new SerializedObject(sourceTerrain);
            var sourceValue = sos.FindProperty("m_ScaleInLightmap").floatValue;
            sos.ApplyModifiedProperties();

            var so = new SerializedObject(targetTerrain);
            so.FindProperty("m_ScaleInLightmap").floatValue = sourceValue / piecesPerAxis;
            so.ApplyModifiedProperties();
        }

        private static void SetTerrainSlicePosition(Terrain sourceTerrain, int piecesPerAxis, int sliceIndex, Transform terrainTransform)
        {
            Vector3 parentPosition = sourceTerrain.GetPosition();

            var sizeY = sourceTerrain.terrainData.size.z;
            var sizeX = sourceTerrain.terrainData.size.x;
            
            var wShift = ComputeShift(piecesPerAxis, sliceIndex, sizeY, sizeX);

            terrainTransform.position = new Vector3(terrainTransform.position.x + wShift.y, terrainTransform.position.y, terrainTransform.position.z + wShift.x);

            terrainTransform.position = new Vector3(terrainTransform.position.x + parentPosition.x, terrainTransform.position.y + parentPosition.y,
                terrainTransform.position.z + parentPosition.z);
        }

        private static Vector2 ComputeShift(int piecesPerAxis, int sliceIndex, float sizeY, float sizeX)
        {
            var spaceShiftX = sizeY / piecesPerAxis;
            var spaceShiftY = sizeX / piecesPerAxis;

            var xWShift = sliceIndex % piecesPerAxis * spaceShiftX;
            var zWShift = sliceIndex / piecesPerAxis * spaceShiftY;
            return new Vector2(xWShift, zWShift);
        }

        private static void SplitControlTexture(TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex, TerrainData sourceTerrainData)
        {
            var sourceControlTextureResolution = sourceTerrainData.alphamapResolution;
            var sourceControlTextureResolutionMinus1 = sourceControlTextureResolution - 1;
            var sourceBaseMapResolution = sourceTerrainData.baseMapResolution;
            var controlTextureResolution = TerrainMath.NextPowerOf2(sourceControlTextureResolution / piecesPerAxis);
            var targetControlTextureResolution = Math.Max(MINIMAL_CONTROL_TEXTURE_RESOLUTION, controlTextureResolution);
            var baseMapResolution = TerrainMath.NextPowerOf2(sourceBaseMapResolution / piecesPerAxis);
            var targetBaseMapResolution = Math.Max(MINIMAL_BASE_TEXTURE_RESOLUTION, baseMapResolution);

            targetTerrainData.alphamapResolution = targetControlTextureResolution;
            targetTerrainData.baseMapResolution = targetBaseMapResolution;

            var sourceControlTextures = sourceTerrainData.GetAlphamaps(0, 0, sourceControlTextureResolution, sourceControlTextureResolution);
            var targetControlTextures = new float[targetControlTextureResolution, targetControlTextureResolution, sourceTerrainData.alphamapLayers];

            var xShift = sliceIndex % piecesPerAxis * sourceControlTextureResolution / piecesPerAxis;
            var yShift = sliceIndex / piecesPerAxis * sourceControlTextureResolution / piecesPerAxis;
            var sampleRatio = targetControlTextureResolution * piecesPerAxis / (float) sourceControlTextureResolution;

            for (var s = 0; s < sourceTerrainData.alphamapLayers; s++)
            {
                for (var x = 0; x < targetControlTextureResolution; x++)
                {
                    if (x % 100 == 0)
                        EditorUtility.DisplayProgressBar("Split terrain", "Split splat", (float) x / targetControlTextureResolution);

                    var xPos = xShift + x / sampleRatio;
                    for (var y = 0; y < targetControlTextureResolution; y++)
                    {
                        var yPos = yShift + y / sampleRatio;
                        var layerIndex = s;
                        var ph = BilinearInterpolator.Interpolate(sourceControlTextures, layerIndex, xPos, yPos, sourceControlTextureResolutionMinus1);

                        targetControlTextures[x, y, s] = ph;
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            targetTerrainData.SetAlphamaps(0, 0, targetControlTextures);
        }

        private static void SplitDetailMap(TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex, TerrainData sourceTerrainData)
        {
            var sourceResolution = sourceTerrainData.detailResolution;
            var targetResolution = Math.Max(MINIMAL_DETAIL_RESOLUTION, sourceResolution / piecesPerAxis);
            var detailResolutionPerPatch = Math.Min(targetResolution, Math.Max(MINIMAL_DETAIL_RESOLUTION, sourceTerrainData.detailResolutionPerPatch));

            targetTerrainData.SetDetailResolution(targetResolution, detailResolutionPerPatch);

            var xShift = sliceIndex % piecesPerAxis * sourceResolution / piecesPerAxis;
            var yShift = sliceIndex / piecesPerAxis * sourceResolution / piecesPerAxis;
            var sampleRatio = targetResolution * piecesPerAxis / (float) sourceResolution;

            var sourceResolutionMinus1 = sourceResolution - 1;

            for (var detLay = 0; detLay < sourceTerrainData.detailPrototypes.Length; detLay++)
            {
                var parentDetail = sourceTerrainData.GetDetailLayer(0, 0, sourceResolution, sourceResolution, detLay);

                var detailResolution = targetResolution;
                var pieceDetail = new int[detailResolution, detailResolution];

                for (var x = 0; x < targetResolution; x++)
                {
                    if (x % 100 == 0)
                        EditorUtility.DisplayProgressBar("Split terrain", "Split details", (float) x / targetResolution);

                    var xPos = xShift + x / sampleRatio;
                    for (var y = 0; y < targetResolution; y++)
                    {
                        var yPos = yShift + y / sampleRatio;
                        var ph = BilinearInterpolator.Interpolate(parentDetail, xPos, yPos, sourceResolutionMinus1);

                        pieceDetail[x, y] = (int) (ph / sampleRatio);
                    }
                }

                EditorUtility.ClearProgressBar();

                targetTerrainData.SetDetailLayer(0, 0, detLay, pieceDetail);
            }
        }

        private static void SplitHeightMap(TerrainData targetTerrainData, int piecesPerAxis, int sliceIndex, TerrainData sourceTerrainData)
        {
            var sourceHeightmapResolutionPlusOne = sourceTerrainData.heightmapResolution;
            var sourceHeightmapResolution = sourceHeightmapResolutionPlusOne - 1;
            var targetHeightmapResolution = TerrainMath.NextPowerOf2(sourceHeightmapResolution / piecesPerAxis);
            targetHeightmapResolution = Math.Max(MINIMAL_HEIGHTMAP_RESOLUTION - 1, targetHeightmapResolution);

            targetTerrainData.heightmapResolution = targetHeightmapResolution;
            targetTerrainData.size = new Vector3(sourceTerrainData.size.x / piecesPerAxis, sourceTerrainData.size.y, sourceTerrainData.size.z / piecesPerAxis);

            var pieceHeight = new float[targetHeightmapResolution + 1, targetHeightmapResolution + 1];

            var sampleRatio = targetHeightmapResolution * piecesPerAxis / (float) sourceHeightmapResolution;
            var xShift = sliceIndex % piecesPerAxis * sourceHeightmapResolution / piecesPerAxis;
            var yShift = sliceIndex / piecesPerAxis * sourceHeightmapResolution / piecesPerAxis;

            var parentHeights = sourceTerrainData.GetHeights(0, 0, sourceHeightmapResolutionPlusOne, sourceHeightmapResolutionPlusOne);
            var parentWidth = parentHeights.GetLength(0);
            var parentWidthMinus1 = parentWidth - 1;

            for (var x = 0; x <= targetHeightmapResolution; x++)
            {
                if (x % 100 == 0)
                    EditorUtility.DisplayProgressBar("Split terrain", "Split height", (float) x / targetHeightmapResolution);

                var xPos = xShift + x / sampleRatio;
                for (var y = 0; y <= targetHeightmapResolution; y++)
                {
                    var yPos = yShift + y / sampleRatio;
                    var ph = BilinearInterpolator.Interpolate(parentHeights, xPos, yPos, parentWidthMinus1);

                    pieceHeight[x, y] = ph;
                }
            }

            EditorUtility.ClearProgressBar();

            targetTerrainData.SetHeights(0, 0, pieceHeight);
        }

        private void SplitTerrainTile(int piecesPerAxis, Terrain sourceTerrain)
        {
            TerrainData sourceTerrainData = sourceTerrain.terrainData;
            var terrains = new Terrain[piecesPerAxis * piecesPerAxis];

            //Split terrain 
            for (var sliceIndex = 0; sliceIndex < terrains.Length; sliceIndex++)
            {
                var targetTerrainData = new TerrainData();
                GameObject terrainGameObject = Terrain.CreateTerrainGameObject(targetTerrainData);

                terrainGameObject.name = $"{sourceTerrain.name}_{sliceIndex}";

                Terrain targetTerrain = terrains[sliceIndex] = terrainGameObject.GetComponent<Terrain>();
                targetTerrain.terrainData = targetTerrainData;

                targetTerrainData.SaveNewAsset("Assets/" + targetTerrain.name + ".asset");

                CopyPrototypes(sourceTerrainData, targetTerrainData, piecesPerAxis, sliceIndex);
                CopyTerrainProperties(sourceTerrain, targetTerrain, piecesPerAxis);
                SetTerrainSlicePosition(sourceTerrain, piecesPerAxis, sliceIndex, terrainGameObject.transform);
                SplitHeightMap(targetTerrainData, piecesPerAxis, sliceIndex, sourceTerrainData);
                SplitControlTexture(targetTerrainData, piecesPerAxis, sliceIndex, sourceTerrainData);
                SplitDetailMap(targetTerrainData, piecesPerAxis, sliceIndex, sourceTerrainData);

                AssetDatabase.SaveAssets();
            }

            SplitTrees(piecesPerAxis, terrains, sourceTerrain);
            AssetDatabase.SaveAssets();

            _layerData.Clear();

            if (_deactivateSource)
                sourceTerrain.gameObject.SetActive(false);
        }

        private void SplitTrees(int terraPieces, Terrain[] tiles, Terrain sourceTerrain)
        {
            if (!_doSplitTrees)
                return;

            var terrainDataTreeInstances = sourceTerrain.terrainData.treeInstances;

            var stepSize = 1f / terraPieces;

            for (var t = 0; t < terrainDataTreeInstances.Length; t++)
            {
                if (t % 100 == 0)
                    EditorUtility.DisplayProgressBar("Split terrain", "Split trees ", (float) t / terrainDataTreeInstances.Length);

                // Get tree instance					
                TreeInstance ti = terrainDataTreeInstances[t];
                Vector3 treePosition = ti.position;

                for (var i = 0; i < tiles.Length; i++)
                {
                    var splitRect = new Rect(i / terraPieces * stepSize, i % terraPieces * stepSize, stepSize, stepSize);

                    if (!splitRect.Contains(new Vector2(treePosition.x, treePosition.z)))
                        continue;

                    // Recalculate new tree position	
                    ti.position = new Vector3((treePosition.x - splitRect.x) * terraPieces, treePosition.y, (treePosition.z - splitRect.y) * terraPieces);

                    // Add tree instance						
                    tiles[i]?.AddTreeInstance(ti);
                    break;
                }
            }
        }
    }
}