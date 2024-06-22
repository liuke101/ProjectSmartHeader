using System.Collections.Generic;
using UnityEngine;

    [CreateAssetMenu (fileName = "TerrainHighlightData", menuName = "ScriptableObjects/TerrainHighlightData", order = 1)]
    public class TerrainHighlightData : ScriptableObject
    {
        public Material HighlightMaterial;
        public List<Material> OriginalMaterials;

        public GameObject HighlightUIComponent;
    }