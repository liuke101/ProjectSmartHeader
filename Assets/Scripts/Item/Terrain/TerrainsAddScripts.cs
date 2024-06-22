using System;
using UnityEngine;

public class TerrainsAddScripts : MonoBehaviour
{
    //遍历儿子, 添加脚本
    [ContextMenu("为所有Terrain Mesh子对象添加高亮脚本")]
    public void AddTerrainHighlightScripts()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            TerrainUIHighlight terrainUIHighlight =
                this.transform.GetChild(i).gameObject.AddComponent<TerrainUIHighlight>();
            TerrainScanFXHighlight terrainScanFXHighlight =
                this.transform.GetChild(i).gameObject.AddComponent<TerrainScanFXHighlight>();
            //这里可以设置参数
        }
    }
}