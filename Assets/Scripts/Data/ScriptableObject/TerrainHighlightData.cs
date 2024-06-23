using System.Collections.Generic;
using UnityEngine;

//地形高亮相关的数据 
[CreateAssetMenu (fileName = "TerrainHighlightData", menuName = "ScriptableObjects/TerrainHighlightData", order = 1)]
public class TerrainHighlightData : ScriptableObject
{
    public Material HighlightMaterial;
    public List<Material> OriginalMaterials;

    public GameObject HighlightUIComponent;
}