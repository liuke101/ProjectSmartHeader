using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JustAssets.TerrainUtility
{
    public static class TerrainExtensions
    {
        public static Vector2 XZ(this Vector3 that)
        {
            return new Vector2(that.x, that.z);
        }

        /// <summary>
        /// If all fields are the same except of the tile offset it is considered similar.
        /// </summary>
        public static bool IsVisuallySame(this TerrainLayer that, TerrainLayer other)
        {
            if (that == null && other != null) 
                return false;
            
            if (that != null && other == null) 
                return false;
            
            if (that == null) 
                return true;

            return that.diffuseTexture == other.diffuseTexture && that.maskMapTexture == other.maskMapTexture &&
                   that.normalMapTexture == other.normalMapTexture && that.diffuseRemapMax == other.diffuseRemapMax &&
                   that.diffuseRemapMin == other.diffuseRemapMin && that.maskMapRemapMax == other.maskMapRemapMax &&
                   that.maskMapRemapMin == other.maskMapRemapMin && Math.Abs(that.metallic - other.metallic) <= float.Epsilon &&
                   Math.Abs(that.normalScale - other.normalScale) <= float.Epsilon && Math.Abs(that.smoothness - other.smoothness) <= float.Epsilon &&
                   that.specular == other.specular && that.tileSize == other.tileSize;
        }

        public static TerrainLayer CloneTerrainLayer(this TerrainLayer layerSource, string layerSourceName, Vector2 newTileOffset)
        {
            var terrainLayer = new TerrainLayer
            {
                tileSize = layerSource.tileSize, diffuseRemapMax = layerSource.diffuseRemapMax,
                diffuseRemapMin = layerSource.diffuseRemapMin, diffuseTexture = layerSource.diffuseTexture,
                maskMapRemapMax = layerSource.maskMapRemapMax, maskMapRemapMin = layerSource.maskMapRemapMin,
                maskMapTexture = layerSource.maskMapTexture, metallic = layerSource.metallic,
                name = layerSourceName, normalMapTexture = layerSource.normalMapTexture, normalScale = layerSource.normalScale,
                smoothness = layerSource.smoothness, specular = layerSource.specular,
                tileOffset = newTileOffset
            };
            return terrainLayer;
        }

        public static string CreateLayerPath(string sourceAssetPath, Vector2 newTileOffset)
        {
            var extension = Path.GetExtension(sourceAssetPath);
            var pathWithoutExtension = Path.ChangeExtension(sourceAssetPath, "");
            pathWithoutExtension = pathWithoutExtension.Substring(0, pathWithoutExtension.Length - 1);
            var newPath = $"{pathWithoutExtension}_{newTileOffset.x}_{newTileOffset.y}{extension}";
            return newPath;
        }

        public static void SaveNewAsset<T>(this T asset, string newAssetPath) where T:Object
        {
            Object currentAssetAtPath = null;

            do
            {
                currentAssetAtPath = AssetDatabase.LoadAssetAtPath<Object>(newAssetPath);

                if (currentAssetAtPath != null)
                {
                    var filePath = newAssetPath.Substring(0, newAssetPath.LastIndexOf('/'));
                    var fileName = Path.GetFileNameWithoutExtension(newAssetPath);
                    var fileNameNumber = new string(fileName.Reverse().TakeWhile(x => char.IsDigit(x)).Reverse().ToArray());
                    var fileNameText = fileName.Substring(0, fileName.Length - fileNameNumber.Length);
                    var fileExtension = Path.GetExtension(newAssetPath);

                    int number;
                    fileNameNumber = int.TryParse(fileNameNumber, out number) ? (number + 1).ToString() : "1";

                    newAssetPath = $"{filePath}/{fileNameText}{fileNameNumber}{fileExtension}";
                }
            } while (currentAssetAtPath != null);

            AssetDatabase.CreateAsset(asset, newAssetPath);
        }
    }
}
