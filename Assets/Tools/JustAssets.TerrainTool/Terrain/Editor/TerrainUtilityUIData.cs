using UnityEngine;

namespace JustAssets.TerrainUtility
{
    [CreateAssetMenu(fileName = "TerrainUtilityUIData", menuName = "ScriptableObjects/TerrainUtilityUIData", order = 1)]
    public class TerrainUtilityUIData : ScriptableObject
    {
        public Texture SplitTerrain;
        public Texture MergeTerrain;
    }
}