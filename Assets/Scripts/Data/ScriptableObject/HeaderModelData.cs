using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public enum EModelType 
{
    门,
    墙,
    楼板,
    半导体应变片,
    光纤光栅温度传感器,
    光纤加速度传感器,
    光纤压力传感器,
    光纤应变传感器,
    压电加速度传感器,
    电类振动速度传感器,
    空气超压传感器,
}

[CreateAssetMenu (fileName = "HeaderMaterialData", menuName = "ScriptableObjects/HeaderMaterialData", order = 4)]
public class HeaderModelData : ScriptableObject
{
    [System.Serializable]
    public struct ModelTypeAsset
    {
        public EModelType Type; //类型
        public Material Material; //材质
        public string NameSubstring; //名字包含的子字符串
    }

    public List<ModelTypeAsset> ModelTypeAssets;
    
    //根据type获取对应的数据
    public Material GetMaterialByModelType(EModelType type)
    {
        return ModelTypeAssets.Find(asset => asset.Type == type).Material;
    }
    
    public string GetNameSubstringByModelType(EModelType type)
    {
        return ModelTypeAssets.Find(asset => asset.Type == type).NameSubstring;
    }
}