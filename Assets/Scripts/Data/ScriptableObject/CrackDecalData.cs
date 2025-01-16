using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrackDecalData", menuName = "ScriptableObjects/CrackDecalData", order = 1)]
public class CrackDecalData : ScriptableObject
{
    [Serializable]
    public struct CrackDecalInfo
    {
        public int DamageLevel; // 损伤等级
        public GameObject CrackDecalPrefab; // 对应的裂纹贴花预制体
        public float DecalSize; // 裂纹贴花的大小
    }

    public List<CrackDecalInfo> CrackDecalInfos;  // 所有损伤等级对应的裂纹贴花数据

    // 根据损伤等级获取裂纹贴花数据
    public CrackDecalInfo GetCrackDecalInfo(int damageLevel)
    {
        return CrackDecalInfos.Find(info => info.DamageLevel == damageLevel);
    }
}