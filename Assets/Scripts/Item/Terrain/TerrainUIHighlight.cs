using System;
using INab.WorldScanFX;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class TerrainUIHighlight : CustomUIHighlight
{
    private TerrainHighlightData terrainHighlightData;
    
    //BUG: 所有UI位置都原点
    protected override void Awake()
    {
        //如果没有UI组件则实例化，并将Canvas_HighlightUI作为父对象
        if (uiComponent == null)
        {
            //从Resources文件夹中加载TerrainHighlightData
            terrainHighlightData = Resources.Load<TerrainHighlightData>("ScriptableObjects/TerrainHighlightData");
            uiComponent = Instantiate(terrainHighlightData?.HighlightUIComponent,
                GameObject.Find("Canvas_HighlightUI").transform, true);
        }

        if (uiComponent != null)
        {
            uiText = uiComponent.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
}