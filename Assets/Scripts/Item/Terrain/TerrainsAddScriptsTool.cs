using System;
using UnityEngine;

public class TerrainsAddScriptsTool : MonoBehaviour
{
    //遍历儿子, 添加脚本
    [ContextMenu("所有Terrain Mesh子对象添加高亮脚本")]
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
    
    [ContextMenu("所有Terrain Mesh子对象移除TerrainUIHighlight脚本")]
    public void RemoveTerrainUIHighlightScript()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            DestroyImmediate(this.transform.GetChild(i).gameObject.GetComponent<TerrainUIHighlight>());
        }
    }
}