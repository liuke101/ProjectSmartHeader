using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JustAssets.TerrainUtility
{
    /// <summary>
    ///     Merges terrain in nxn pieces.
    /// </summary>
    public sealed class MergeTerrain : IHandler<Terrain>
    {
        private bool _deactivateSource = true;
        private bool _ignoreLayerOffset;
        private Terrain[] _selection = Array.Empty<Terrain>();
        private EInvalidReason _valid;
        private int _layerCountToExpect;
        private const int MAXIMUM_HEIGHTMAP_RESOLUTION = 4097;

        private const int MAXIMUM_CONTROL_TEXTURE_RESOLUTION = 4096;

        private const string APPROXIMATION_MERGE = 
            "Merging a square amount of slices (selection: {0} slices) is possible but results in an approximated result. To archive a perfect merge result, please select x² terrain slices (e.g. 4, 16, 64, ...).";
            
        private const string WRONG_SELECTION =
            "You need to select at least 4 terrains. You need to select n^2 terrains (4, 16, 64, 256, ...). You selected {0} pieces.";

        private const string INVALID_SHAPE =
            "You need to select a square shape of terrains.";

        private const string TOO_MANY_LAYERS =
            "The resulting terrain would generate too many terrain layers ({0}) and be larger than 1 GiB. Try turning on 'ignore terrain offset' to reduce the resulting layers. Though, this will shift the offset of the painted textures.";

        private const int MAXIMUM_BASEMAP_TEXTURE_RESOLUTION = 4096;

        public Terrain[] Selection
        {
            get => _selection;
            set
            {
                _selection = value;
                _valid = Validate(out _layerCountToExpect);
            }
        }

        public int LayerCountToExpect
        {
            get => _layerCountToExpect;
            set => _layerCountToExpect = value;
        }

        public enum EInvalidReason
        {
            AllFine,
            NothingSelected,
            TooLittleSelected,
            WrongCountSelected,
            NotSquare,
            TooManyLayers,

            InvalidShape
        }

        public object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return _deactivateSource;
                    case 1:
                        return _ignoreLayerOffset;
                }

                throw new ArgumentOutOfRangeException(nameof(index));
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _deactivateSource = (bool) value;
                        break;
                    case 1:
                        _ignoreLayerOffset = (bool) value;
                        break;
                }

                _valid = Validate(out _layerCountToExpect);
            }
        }

        public bool IsValid(out string reason)
        {
            reason = null;
            switch (_valid)
            {
                case EInvalidReason.NothingSelected:
                    reason = "Select some terrain.";
                    return false;

                case EInvalidReason.TooLittleSelected:
                    reason = string.Format(WRONG_SELECTION, Selection.Length);
                    return false;

                case EInvalidReason.WrongCountSelected:
                    reason = string.Format(WRONG_SELECTION, Selection.Length);
                    return false;

                case EInvalidReason.InvalidShape:
                    reason = string.Format(INVALID_SHAPE);
                    return false;

                case EInvalidReason.NotSquare:
                    reason = string.Format(APPROXIMATION_MERGE, Selection.Length);
                    return true;

                case EInvalidReason.TooManyLayers:
                    reason = string.Format(TOO_MANY_LAYERS, _layerCountToExpect);
                    return false;
            }

            return true;
        }

        public void Execute()=>Merge();

        public void Merge()
        {
            if (_valid != EInvalidReason.AllFine && _valid != EInvalidReason.NotSquare)
            {
                EditorUtility.DisplayDialog("Error", string.Format(WRONG_SELECTION, Selection.Length), "Cancel");
                return;
            }

            var terrainsPerAxis = (int) Math.Sqrt(Selection.Length);
            var sorted = DetermineTerrainPositions(Selection, terrainsPerAxis);

            if (sorted.Cast<Terrain>().Any(terrain => terrain == null))
            {
                EditorUtility.DisplayDialog("Error", "You're terrain slices need to be aligned in a grid to be merged.", "Cancel");
                return;
            }

            MergeTerrainTiles(sorted, Selection, terrainsPerAxis);

            EditorUtility.ClearProgressBar();
        }

        private static int Clamp(int xTarget, int maximum)
        {
            return xTarget < 0 ? 0 : xTarget >= maximum ? maximum - 1 : xTarget;
        }

        private void CopyChildren(Terrain[,] sorted, Terrain targetTerrain, int terrainsPerAxis)
        {
            for (var ty = 0; ty < terrainsPerAxis; ty++)
            {
                for (var tx = 0; tx < terrainsPerAxis; tx++)
                {
                    foreach (Transform child in sorted[ty, tx].transform)
                    {
                        UnityEditor.Selection.activeGameObject = child.gameObject;
                        Unsupported.CopyGameObjectsToPasteboard();
                        Unsupported.PasteGameObjectsFromPasteboard();
                        GameObject copy = UnityEditor.Selection.activeGameObject;
                        copy.transform.SetParent(targetTerrain.transform);
                        copy.name = child.name;
                    }
                }
            }
        }

        private void CopyParentProperties(Terrain[] sourceTerrains, Terrain targetTerrain)
        {
            targetTerrain.basemapDistance = sourceTerrains.Average(x => x.basemapDistance);
            targetTerrain.shadowCastingMode = sourceTerrains.First().shadowCastingMode;
            targetTerrain.detailObjectDensity = sourceTerrains.Average(x => x.detailObjectDensity);
            targetTerrain.detailObjectDistance = sourceTerrains.Average(x => x.detailObjectDistance);
            targetTerrain.heightmapMaximumLOD = (int) sourceTerrains.Average(x => x.heightmapMaximumLOD);
            targetTerrain.heightmapPixelError = sourceTerrains.Average(x => x.heightmapPixelError);
            targetTerrain.treeBillboardDistance = sourceTerrains.Average(x => x.treeBillboardDistance);
            targetTerrain.treeCrossFadeLength = sourceTerrains.Average(x => x.treeCrossFadeLength);
            targetTerrain.treeDistance = sourceTerrains.Average(x => x.treeDistance);
            targetTerrain.treeMaximumFullLODCount = (int) sourceTerrains.Average(x => x.treeMaximumFullLODCount);
            targetTerrain.bakeLightProbesForTrees = sourceTerrains.Any(x => x.bakeLightProbesForTrees);
            targetTerrain.drawInstanced = sourceTerrains.Any(x => x.drawInstanced);
            targetTerrain.reflectionProbeUsage = sourceTerrains.First().reflectionProbeUsage;
            targetTerrain.realtimeLightmapScaleOffset = sourceTerrains.First().realtimeLightmapScaleOffset;
            targetTerrain.lightmapScaleOffset = sourceTerrains.First().lightmapScaleOffset;

            SetLightmapScale(targetTerrain, (int) Math.Sqrt(sourceTerrains.Length), sourceTerrains.First());

            targetTerrain.terrainData.wavingGrassAmount = sourceTerrains.Average(x => x.terrainData.wavingGrassAmount);
            targetTerrain.terrainData.wavingGrassSpeed = sourceTerrains.Average(x => x.terrainData.wavingGrassSpeed);
            targetTerrain.terrainData.wavingGrassStrength = sourceTerrains.Average(x => x.terrainData.wavingGrassStrength);
            targetTerrain.terrainData.wavingGrassTint = sourceTerrains.First().terrainData.wavingGrassTint;
        }

        private static void CreateHeightmap(Terrain[] sourceTerrains, Terrain[,] sorted, Terrain targetTerrain, int piecesPerAxis)
        {
            TerrainData targetTerrainData = targetTerrain.terrainData;
            var maxHeightmapResolution = sourceTerrains.Max(x => x.terrainData.heightmapResolution);
            maxHeightmapResolution = Math.Min(maxHeightmapResolution, MAXIMUM_HEIGHTMAP_RESOLUTION);

            var closestPowerOf2 = TerrainMath.ClosestPowerOf2((maxHeightmapResolution - 1) * piecesPerAxis);
            var targetHeightmapResolution = Math.Min(MAXIMUM_HEIGHTMAP_RESOLUTION, closestPowerOf2 + 1);
            targetTerrainData.heightmapResolution = targetHeightmapResolution;

            var newTerrainHeight = ComputeNewTerrainHeight(sourceTerrains, out var newOffset);
            targetTerrainData.size = new Vector3(sourceTerrains.Max(x => x.terrainData.size.x) * piecesPerAxis, newTerrainHeight,
                sourceTerrains.Max(x => x.terrainData.size.z) * piecesPerAxis);

            var mergedHeights = new float[targetHeightmapResolution, targetHeightmapResolution];
            var targetHeight = targetTerrainData.size.y;
            
            for (var sliceIndex = 0; sliceIndex < piecesPerAxis * piecesPerAxis; sliceIndex++)
            {
                var tx = sliceIndex % piecesPerAxis;
                var ty = sliceIndex / piecesPerAxis;
                var terrain = sorted[ty, tx];
                TerrainData terrainData = terrain.terrainData;
                var sourceResolution = terrainData.heightmapResolution;
                var sourceResolutionMinus1 = sourceResolution - 1;
                var parentHeights = terrainData.GetHeights(0, 0, sourceResolution, sourceResolution);
                var sliceHeight = terrainData.size.y;
                var heightOffsetToFirstSlide = terrain.GetPosition().y - newOffset;
                var targetRegionWidth = (targetHeightmapResolution - 1) / (float)piecesPerAxis;

                for (var x = 0; x <= targetRegionWidth; x++)
                {
                    for (var y = 0; y <= targetRegionWidth; y++)
                    {
                        var xPos = x / (float) targetRegionWidth * sourceResolution;
                        var yPos = y / (float) targetRegionWidth * sourceResolution;

                        var ph = BilinearInterpolator.Interpolate(parentHeights, xPos, yPos, sourceResolutionMinus1) * sliceHeight + heightOffsetToFirstSlide;
                        ph /= targetHeight;

                        mergedHeights[(int)Math.Ceiling(x + targetRegionWidth * tx), (int)Math.Ceiling(y + targetRegionWidth * ty)] = ph;
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            // Set heightmap to child
            targetTerrain.terrainData.SetHeights(0, 0, mergedHeights);
            var terrainTransform = targetTerrain.transform;
            var transformPosition = terrainTransform.position;
            transformPosition.y = newOffset;
            terrainTransform.position = transformPosition;
        }

        private static float ComputeNewTerrainHeight(Terrain[] sourceTerrains, out float newOffset)
        {
            var biggestHeight = float.NegativeInfinity;
            var smallestHeight = float.PositiveInfinity;

            foreach (var sourceTerrain in sourceTerrains)
            {
                var terrainData = sourceTerrain.terrainData;
                var heightmapResolution = terrainData.heightmapResolution;
                var heights = terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);
                var localMax = 0f;
                var localMin = 1f;

                foreach (var height in heights)
                {
                    if (height > localMax)
                        localMax = height;
                    if (height < localMin)
                        localMin = height;
                }

                localMax = localMax * terrainData.size.y + sourceTerrain.GetPosition().y;
                localMin = localMin * terrainData.size.y + sourceTerrain.GetPosition().y;

                if (localMax > biggestHeight)
                    biggestHeight = localMax;

                if (localMin < smallestHeight)
                    smallestHeight = localMin;
            }
            
            var userDefinedMaxHeight = sourceTerrains.Max(x => x.terrainData.size.y);
            var computedMaxHeight = biggestHeight - smallestHeight;

            var newHeight = Math.Max(computedMaxHeight, userDefinedMaxHeight);
            newOffset = smallestHeight;

            return newHeight;
        }

        private static Terrain[,] DetermineTerrainPositions(Terrain[] sourceTerrains, int piecesPerAxis)
        {
            var sorted = new Terrain[piecesPerAxis, piecesPerAxis];

            var bounds = new Bounds(sourceTerrains.First().GetPosition(), Vector3.zero);
            for (var index = 1; index < sourceTerrains.Length; index++)
            {
                Terrain sourceTerrain = sourceTerrains[index];
                Vector3 pos = sourceTerrain.GetPosition();
                bounds.Encapsulate(pos);
            }

            foreach (Terrain sourceTerrain in sourceTerrains)
            {
                Vector3 boundsMin = sourceTerrain.GetPosition() - bounds.min;
                var index = new Vector2(boundsMin.x / bounds.size.x, boundsMin.z / bounds.size.z);

                var x = (int) Math.Round(index.x * (piecesPerAxis - 1));
                var y = (int) Math.Round(index.y * (piecesPerAxis - 1));

                if (x >= sorted.GetLength(0) || x < 0 || y >= sorted.GetLength(1) || y < 0)
                    return null;

                sorted[x, y] = sourceTerrain;
            }

            return sorted;
        }

        private void FillMergeDetails(TerrainData td, Terrain[,] sorted, Terrain[] sourceTerrains, Terrain targetTerrain, int terrainsPerAxis)
        {
            var maxDetailResolution = sourceTerrains.Max(x => x.terrainData.detailResolution);
            var maxDetailResolutionPerPath = sourceTerrains.Max(x => x.terrainData.detailResolutionPerPatch);

            var newDetailResolution = maxDetailResolution * terrainsPerAxis;

            td.SetDetailResolution(newDetailResolution, maxDetailResolutionPerPath);

            for (var targetLayer = 0; targetLayer < targetTerrain.terrainData.detailPrototypes.Length; targetLayer++)
            {
                var targetDetails = new int[newDetailResolution, newDetailResolution];
                DetailPrototype targetDetailObject = targetTerrain.terrainData.detailPrototypes[targetLayer];

                for (var ty = 0; ty < terrainsPerAxis; ty++)
                {
                    for (var tx = 0; tx < terrainsPerAxis; tx++)
                    {
                        TerrainData currentTerrainData = sorted[ty, tx].terrainData;
                        var currentDetailResolution = currentTerrainData.detailResolution;

                        var sourceLayerIndices = IndicesOf(currentTerrainData.detailPrototypes, targetDetailObject,
                            (a, b) => Equals(a?.prototypeTexture, b?.prototypeTexture) && Equals(a?.prototype, b?.prototype));

                        foreach (var sourceLayerIndex in sourceLayerIndices)
                        {
                            var sourceDetails = currentTerrainData.GetDetailLayer(0, 0, currentDetailResolution, currentDetailResolution, sourceLayerIndex);

                            // Shift calc

                            var xShift = tx * maxDetailResolution;
                            var yShift = ty * maxDetailResolution;
                            var scale = maxDetailResolution / currentDetailResolution;

                            // iterate				
                            for (var x = 0; x < currentDetailResolution; x++)
                            {
                                for (var y = 0; y < currentDetailResolution; y++)
                                    RunDetailsKernel(sourceDetails, x, y, scale, xShift, yShift, newDetailResolution, targetDetails, terrainsPerAxis);
                            }
                        }

                        EditorUtility.ClearProgressBar();
                    }
                }

                // Set heightmap to child
                targetTerrain.terrainData.SetDetailLayer(0, 0, targetLayer, targetDetails);
            }
        }

        private void FillSplatMaps(TerrainData td, Terrain[,] sorted, Terrain targetTerrain, int piecesPerAxis)
        {
            var terrainDatas = sorted.Cast<Terrain>().Select(x => x.terrainData).ToArray();
            var sourceAlphamapResolution = terrainDatas.Max(x => x.alphamapResolution);
            var sourceBasemapResolution = terrainDatas.Max(x => x.baseMapResolution);

            var newAlphamapResolution = Math.Min(MAXIMUM_CONTROL_TEXTURE_RESOLUTION, sourceAlphamapResolution * piecesPerAxis);
            var newBasemapResolution = Math.Min(MAXIMUM_BASEMAP_TEXTURE_RESOLUTION, sourceBasemapResolution * piecesPerAxis);

            td.alphamapResolution = newAlphamapResolution;
            td.baseMapResolution = newBasemapResolution;

            var result = new float[newAlphamapResolution, newAlphamapResolution, targetTerrain.terrainData.terrainLayers.Length];

            var progress = 0;
            var targetRegionWidth = newAlphamapResolution / piecesPerAxis;
            var max = terrainDatas.Max(x => x.alphamapLayers) * targetRegionWidth * targetRegionWidth * piecesPerAxis * piecesPerAxis;

            for (var sliceIndex = 0; sliceIndex < piecesPerAxis * piecesPerAxis; sliceIndex++)
            {
                var tx = sliceIndex % piecesPerAxis;
                var ty = sliceIndex / piecesPerAxis;
                var terrain = sorted[ty, tx];
                TerrainData terrainData = terrain.terrainData;
                var sourceResolution = terrainData.alphamapResolution;
                var sourceResolutionMinus1 = sourceResolution - 1;
                var parentHeights = terrainData.GetAlphamaps(0, 0, sourceResolution, sourceResolution);

                var alphamapLayers = terrainData.alphamapLayers;

                var localPosition = terrain.GetPosition().XZ();
                
                for (var sourceIndex = 0; sourceIndex < alphamapLayers; sourceIndex++)
                {
                    var targetIndex = FindTargetLayer(targetTerrain, localPosition, terrainData.terrainLayers[sourceIndex]);

                    if (targetIndex < 0)
                        continue;
                    
                    for (var x = 0; x < targetRegionWidth; x++)
                    {
                        for (var y = 0; y < targetRegionWidth; y++)
                        {
                            var xPos = x / (float) targetRegionWidth * sourceResolution;
                            var yPos = y / (float) targetRegionWidth * sourceResolution;

                            var ph = BilinearInterpolator.Interpolate(parentHeights, sourceIndex, xPos, yPos, sourceResolutionMinus1);

                            result[x + targetRegionWidth * tx, y + targetRegionWidth * ty, targetIndex] += ph;

                            if (progress % 1000 == 0)
                                EditorUtility.DisplayProgressBar("Writing alpha map value", $"Working {progress}/{max}...", progress / (float) max);
                            progress++;
                        }
                    }
                }
            }

            EditorUtility.ClearProgressBar();

            // Set heightmap to child
            targetTerrain.terrainData.SetAlphamaps(0, 0, result);
        }

        private int FindTargetLayer(Terrain targetTerrain, Vector2 localPosition, TerrainLayer terrainDataTerrainLayer)
        {
            var targetTerrainOrigin = targetTerrain.GetPosition().XZ();

            var indices = IndicesOf(targetTerrain.terrainData.terrainLayers, terrainDataTerrainLayer, (sourceLayer, targetLayer) => IsMatchingLayer(targetLayer, sourceLayer, localPosition, targetTerrainOrigin, _ignoreLayerOffset));
            return indices.Count > 0 ? indices[0] : -1;
        }
  
        private static bool IsMatchingLayer(TerrainLayer targetLayer, TerrainLayer sourceLayer, Vector2 localPosition,
            Vector2 targetTerrainOrigin, bool ignoreLayerOffset)
        {
            if (!targetLayer.IsVisuallySame(sourceLayer)) 
                return false;

            if (ignoreLayerOffset || sourceLayer == null) 
                return true;

            return IsSameOffset(targetLayer, sourceLayer, out Vector2 _);
        }

        private static bool IsOffsetSimilar(Vector2 newLayerOffset, Vector2 layerTileOffset, Vector2 bounds)
        {
            const float epsilon = 0.001f;
            var xOffset = (layerTileOffset.x - newLayerOffset.x + bounds.x) % bounds.x;
            var yOffset = (layerTileOffset.y - newLayerOffset.y + bounds.y) % bounds.y;

            var xCloseToUpperBound = Math.Abs(xOffset - bounds.x) < epsilon;
            var yCloseToUpperBound = Math.Abs(yOffset - bounds.y) < epsilon;

            var xCloseToLowerBound = Math.Abs(xOffset) < epsilon;
            var yCloseToLowerBound = Math.Abs(yOffset) < epsilon;
            
            var xIsClose = xCloseToUpperBound || xCloseToLowerBound;
            var yIsClose = yCloseToUpperBound || yCloseToLowerBound;
            
            return xIsClose && yIsClose;
        }

        private List<int> IndicesOf<T>(T[] stack, T needle, Func<T, T, bool> equals = null)
        {
            if (equals == null)
                equals = (a, b) => Equals(a, b);

            var matches = new List<int>();

            for (var i = 0; i < stack.Length; i++)
            {
                if (equals(needle, stack[i]))
                    matches.Add(i);
            }

            return matches;
        }

        private EInvalidReason Validate(out int layerCount)
        {
            layerCount = 0;

            var selectionCount = Selection.Length;

            if (selectionCount < 4)
                return EInvalidReason.TooLittleSelected;

            var terrainsPerAxis = Math.Sqrt(Selection.Length);
            
            if (terrainsPerAxis - Math.Floor(terrainsPerAxis) > 0.001f)
                return EInvalidReason.WrongCountSelected;

            if (!TerrainMath.IsPowerOf2((int)terrainsPerAxis))
                return EInvalidReason.NotSquare;

            var sorted = DetermineTerrainPositions(Selection, (int) terrainsPerAxis);

            if (sorted == null)
                return EInvalidReason.InvalidShape;

            layerCount = GetTerrainLayersAndReset(sorted, (int)terrainsPerAxis, _ignoreLayerOffset, false).Length;
            
            if (layerCount > 128)
                return EInvalidReason.TooManyLayers;

            return EInvalidReason.AllFine;
        }

        private static T[] MergeArray<T>(Terrain[] sourceTerrains, Func<Terrain, T[]> retrieve)
        {
            return sourceTerrains.SelectMany(retrieve).Distinct().ToArray();
        }

        private TreePrototype[] MergeSimiliarPrototypes(TreePrototype[] prototypes)
        {
            var result = new List<TreePrototype>();

            foreach (TreePrototype prototype in prototypes)
            {
                TreePrototype alreadyCreated = result.FirstOrDefault(x => x.prefab != null && x.prefab == prototype.prefab);

                if (alreadyCreated == null)
                {
                    result.Add(prototype);
                    alreadyCreated = prototype;
                }

                alreadyCreated.bendFactor = Math.Max(alreadyCreated.bendFactor, prototype.bendFactor);
            }

            return result.ToArray();
        }

        private DetailPrototype[] MergeSimiliarPrototypes(DetailPrototype[] prototypes)
        {
            var result = new List<DetailPrototype>();

            foreach (DetailPrototype prototype in prototypes)
            {
                DetailPrototype alreadyCreated = result.FirstOrDefault(x =>
                    x.prototypeTexture != null && x.prototypeTexture == prototype.prototypeTexture ||
                    x.prototype != null && x.prototype == prototype.prototype);

                if (alreadyCreated == null)
                {
                    result.Add(prototype);
                    alreadyCreated = prototype;
                }

                alreadyCreated.minHeight = Math.Min(alreadyCreated.minHeight, prototype.minHeight);
                alreadyCreated.minWidth = Math.Min(alreadyCreated.minWidth, prototype.minWidth);
                alreadyCreated.maxHeight = Math.Max(alreadyCreated.maxHeight, prototype.maxHeight);
                alreadyCreated.maxWidth = Math.Max(alreadyCreated.maxWidth, prototype.maxWidth);
            }

            return result.ToArray();
        }

        private void MergeTerrainTiles(Terrain[,] sorted, Terrain[] sourceTerrains, int terrainsPerAxis)
        {
            var terrainData = new TerrainData();
            GameObject tgo = Terrain.CreateTerrainGameObject(terrainData);
            var targetTerrain = tgo.GetComponent<Terrain>();
            targetTerrain.terrainData = terrainData;
            tgo.name = sourceTerrains.First().name + " ";
            terrainData.SaveNewAsset("Assets/" + targetTerrain.name + ".asset");

            var origin = sorted[0, 0].GetPosition();

            // Assign splatmaps
            targetTerrain.terrainData.terrainLayers = GetTerrainLayersAndReset(sorted, terrainsPerAxis, _ignoreLayerOffset);
            
            // Assign detail prototypes
            var prototypes = MergeArray(sourceTerrains, x => x.terrainData.detailPrototypes);
            targetTerrain.terrainData.detailPrototypes = MergeSimiliarPrototypes(prototypes);

            // Assign tree information
            var treePrototypes = MergeArray(sourceTerrains, x => x.terrainData.treePrototypes);
            targetTerrain.terrainData.treePrototypes = MergeSimiliarPrototypes(treePrototypes);

            CopyParentProperties(sourceTerrains, targetTerrain);

            //Start processing it			
            tgo.transform.position = origin;

            //Copy heightmap											
            CreateHeightmap(sourceTerrains, sorted, targetTerrain, terrainsPerAxis);

            // Merge splat map
            FillSplatMaps(terrainData, sorted, targetTerrain, terrainsPerAxis);

            // Merge detail map
            FillMergeDetails(terrainData, sorted, sourceTerrains, targetTerrain, terrainsPerAxis);

            AssetDatabase.SaveAssets();

            // Merge tree data
            MergeTrees(sorted, targetTerrain, terrainsPerAxis);

            CopyChildren(sorted, targetTerrain, terrainsPerAxis);

            AssetDatabase.SaveAssets();

            if (_deactivateSource)
                foreach (Terrain sourceTerrain in sourceTerrains)
                    sourceTerrain.gameObject.SetActive(false);
        }

        private static TerrainLayer[] GetTerrainLayersAndReset(Terrain[,] sourceTerrains, int terrainsPerAxis, bool ignoreLayerOffset, bool persistAsset = true)
        {
            Terrain originTerrain = sourceTerrains[0, 0];
            var origin = originTerrain.GetPosition().XZ();
            
            var result = new List<TerrainLayer>();
            for (int ty = 0; ty < terrainsPerAxis; ty++)
            {
                for (int tx = 0; tx < terrainsPerAxis; tx++)
                {
                    var currentSlice = sourceTerrains[ty, tx];

                    if (currentSlice?.terrainData?.terrainLayers == null)
                        continue;

                    for (int i = 0; i < currentSlice.terrainData.terrainLayers.Length; i++)
                    {
                        var layer = currentSlice.terrainData.terrainLayers[i];
                        
                        bool any = false;
                        Vector2 finalLayerOffset = layer.tileOffset;
                        foreach (TerrainLayer x in result)
                        {
                            if (x.IsVisuallySame(layer) && (ignoreLayerOffset || IsSameOffset(x, layer, out finalLayerOffset)))
                            {
                                any = true;
                                break;
                            }
                        }

                        if (!any)
                        {
                            var newLayer = layer.CloneTerrainLayer($"{layer.name}_x{finalLayerOffset.x:F3}_y{finalLayerOffset.y:F3}", finalLayerOffset);

                            if (persistAsset)
                            {
                                var newAssetPath = TerrainExtensions.CreateLayerPath(AssetDatabase.GetAssetPath(layer), finalLayerOffset);
                                newLayer.SaveNewAsset(newAssetPath);
                            }

                            result.Add(newLayer);

                        }
                    }
                }
            }

            return result.ToArray();
        }

        private static bool IsSameOffset(TerrainLayer targetLayer, TerrainLayer sourceLayer, out Vector2 similarityOffset)
        {
            var tileSize = sourceLayer.tileSize;

            var sourceOffset = sourceLayer.tileOffset / tileSize;
            var targetOffset = targetLayer.tileOffset / tileSize;

            var deltaOffset = sourceOffset - targetOffset;

            similarityOffset = sourceLayer.tileOffset - deltaOffset * tileSize;

            return IsOffsetSimilar(similarityOffset, targetLayer.tileOffset, tileSize);
        }

        private void MergeTrees(Terrain[,] sorted, Terrain targetTerrain, int terrainsPerAxis)
        {
            for (var ty = 0; ty < terrainsPerAxis; ty++)
            {
                for (var tx = 0; tx < terrainsPerAxis; tx++)
                {
                    var terrainDataTreeInstances = sorted[ty, tx].terrainData.treeInstances;
                    for (var t = 0; t < terrainDataTreeInstances.Length; t++)
                    {
                        // Get tree instance					
                        TreeInstance ti = terrainDataTreeInstances[t];

                        // Recalculate new tree position	
                        ti.position = new Vector3(ti.position.x / terrainsPerAxis + ty / (float) terrainsPerAxis, ti.position.y,
                            ti.position.z / terrainsPerAxis + tx / (float) terrainsPerAxis);

                        // Add tree instance						
                        targetTerrain.AddTreeInstance(ti);
                    }
                }
            }
        }

        private static void RunDetailsKernel(int[,] sourceDetails, int x, int y, int scale, int xShift, int yShift, int newDetailResolution,
            int[,] targetDetails, int terrainsPerAxis)
        {
            var ph = sourceDetails[x, y];

            if (ph == 0)
                return;

            var scaleBoundUpper = Math.Max(1, scale / terrainsPerAxis);
            var scaleBoundLower = -scale / terrainsPerAxis;

            for (var xScale = scaleBoundLower; xScale < scaleBoundUpper; xScale++)
            {
                for (var yScale = scaleBoundLower; yScale < scaleBoundUpper; yScale++)
                {
                    var xTarget = x * scale + xShift + xScale;
                    var yTarget = y * scale + yShift + yScale;

                    xTarget = Clamp(xTarget, newDetailResolution);
                    yTarget = Clamp(yTarget, newDetailResolution);

                    targetDetails[xTarget, yTarget] += Math.Max(1, ph / scale);
                }
            }
        }

        private void SetLightmapScale(Terrain targetTerrain, float terrainPiecesPerAxis, Terrain sourceTerrain)
        {
            const string scaleInLightmap = "m_ScaleInLightmap";

            var sos = new SerializedObject(sourceTerrain);
            var sourceValue = sos.FindProperty(scaleInLightmap).floatValue;
            sos.ApplyModifiedProperties();

            var so = new SerializedObject(targetTerrain);
            so.FindProperty(scaleInLightmap).floatValue = sourceValue * terrainPiecesPerAxis;
            so.ApplyModifiedProperties();
        }
    }
}