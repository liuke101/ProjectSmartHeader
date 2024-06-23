using System;
using System.Collections.Generic;
using Best.HTTP.Caching;
using UnityEngine;

public enum EBombType 
{
    温压弹,
    堵口爆,
    核爆,
}

//炸弹类型相关的数据
[CreateAssetMenu (fileName = "BombTypeData", menuName = "ScriptableObjects/BombTypeData", order = 0)]
public class BombTypeData : ScriptableObject
{
    [Serializable]
    public struct BombTypeAsset
    {
        public EBombType Type; //类型
        public GameObject BombObject; //炸弹对象
        public GameObject PointerVFX; //指示特效
        public GameObject ExplosionVFX; //爆炸特效
        public GameObject BombHoleDecal; //炸弹贴花
    }

    public List<BombTypeAsset> BombTypeAssets;
    
    //根据type获取对应的数据
    public BombTypeAsset GetAssetByBombType(EBombType type)
    {
        return BombTypeAssets.Find(asset => asset.Type == type);
    }

}