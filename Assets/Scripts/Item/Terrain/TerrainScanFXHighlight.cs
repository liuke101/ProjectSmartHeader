using System.Collections;
using System.Collections.Generic;
using INab.WorldScanFX;
using UnityEngine;

[DisallowMultipleComponent]
public class TerrainScanFXHighlight : ScanFXHighlight
{
    private TerrainHighlightData terrainHighlightData;
    
    private void Awake()
    {
        renderers.Add(GetComponent<MeshRenderer>());
        
        //从Resources文件夹中加载TerrainHighlightData
        terrainHighlightData = Resources.Load<TerrainHighlightData>("ScriptableObjects/TerrainHighlightData");
    }

    //替换源材质为高亮材质
    public void ChangeOriginMaterial()
    {
        //扫描到时
        renderers[0].materials = new[] { terrainHighlightData?.HighlightMaterial };
    }
    
    //恢复为源材质
    public void ReverseOriginMaterial()
    {
        renderers[0].materials = terrainHighlightData?.OriginalMaterials.ToArray();
    }
    
    protected override IEnumerator EffectEnumerator()
    {
        yield return null;
        float value;
        float elapsedTime = 0f;
        effectIsPlaying = true;
        
        //扫描到时替换源材质为高亮材质
        ChangeOriginMaterial();
        
        if (materialPropertyBlock == null) { materialPropertyBlock = new MaterialPropertyBlock(); }
        
        while (elapsedTime < highlightDuration)
        {
            elapsedTime += Time.deltaTime;
        
            float effectTime = elapsedTime / highlightDuration;
            value = curve.Evaluate(effectTime);
        
            UpdateHighlightValue(value);
        
            yield return null;
        }
        
        effectIsPlaying = false;
        
        //结束后恢复为源材质
        ReverseOriginMaterial();
    }
}