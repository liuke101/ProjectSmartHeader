using System.Collections.Generic;
using UnityEngine;

//打击等级相关的数据
[CreateAssetMenu (fileName = "StrikeLevelData", menuName = "ScriptableObjects/StrikeLevelData", order = 0)]
public class StrikeLevelData : ScriptableObject
{
    //索引0~3代表0-4级
    public List<float> ShockWaveRange;  //冲击波范围、相机震动影响范围，ScanFX扫描范围（半径）
    public List<float> BombHoleDecalSize; //弹坑贴花大小
    
    //TODO:控制HS_ShakeOnCollision的其它参数
}