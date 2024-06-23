using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BombHoleDecal : MonoBehaviour
{
    public List<Material> DecalMaterials;
    private DecalProjector decalProjector;
    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();
        
        //随机设置贴花材质
        if (decalProjector != null) decalProjector.material = DecalMaterials[UnityEngine.Random.Range(0, DecalMaterials.Count)];
    }

    public void SetRandomDecalMaterial()
    {
        
    }
}