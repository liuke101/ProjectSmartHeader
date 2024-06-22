using System.Collections;
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
    
    protected override IEnumerator EffectEnumerator()
    {
        float value;
        float elapsedTime = 0f;
        effectIsPlaying = true;
        
        //扫描到时替换源材质为高亮材质
        renderers[0].materials = new [] { terrainHighlightData.HighlightMaterial };

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
        renderers[0].materials = terrainHighlightData.OriginalMaterials.ToArray(); 
    }
}