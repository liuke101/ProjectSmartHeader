using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu (fileName = "StrikeLevelData", menuName = "ScriptableObject/StrikeLevelData", order = 0)]
public class StrikeLevelData : ScriptableObject
{
    //索引0~3代表0-4级
    public List<float> Range;  //冲击波范围、相机震动影响范围，ScanFX扫描范围（半径）
    
    //TODO:控制HS_ShakeOnCollision的其它参数
}