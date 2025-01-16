using System.Collections.Generic;
using INab.WorldScanFX.URP;
using JsonStruct;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using TMPro;

/// <summary>
/// 炸弹生成控制器，用于管理和生成炸弹
/// </summary>
public class SpawnBombController : MonoSingleton<SpawnBombController>
{
    [Header("主变量")]
    public List<GameObject> BombObjects; // 炸弹对象列表
    public ScanFX ScanFX; // 扫描特效组件
    public StrikeLevelData StrikeLevelData; // 打击等级数据
    public BombTypeData BombTypeData; // 炸弹类型数据
    
    [Header("测试生成参数")]
    public List<Vector3> SpawnPositions; // 生成位置列表
    public float TargetRange = 500.0f; // 目标范围
    
    [Header("委托/事件")]
    public UnityEvent<ExplosiveSourceData> ExplosionDataEvent; // 用于向UI发送爆源数据的事件
    public UnityEvent<DamageData> DamageDataEvent; // 用于向UI发送损伤数据的事件
    
    [Header("UI组件")]
    public TextMeshProUGUI distanceText; // 显示距离的文本组件
    public Transform modelTransform;  // 用于计算距离的模型 Transform
    
    public DamageDecalManager damageDecalManager;


    protected override void Awake()
    {
        base.Awake(); 
        ScanFX = GetComponent<ScanFX>();
    }
    
    /// <summary>
    /// 随机生成炸弹，用于测试
    /// </summary>
    public void SpawnBomb()
    {
        if(BombObjects.Count == 0)
        {
            Debug.LogError("BombObjects 为空");
            return;
        }
        
        // 随机生成打击等级和生成位置
        int StrikeLevel = Random.Range(1, 5);
        Vector3 SpawnPosion = SpawnPositions[Random.Range(0, SpawnPositions.Count)];
        Quaternion SpwanRotation = Quaternion.AngleAxis(-80, Vector3.right);  // 四元数绕x轴旋转-80度
        
        Vector3 TargetPosition = new Vector3(Random.Range(-TargetRange, TargetRange), 0, Random.Range(-TargetRange, TargetRange));
        
        // 根据类型生成对应的炸弹
        GameObject BombObject = null;
        GameObject BombPointerVFX = null;
        
        switch (Random.Range(0, 3)) // 随机类型
        {
            case 0:
                BombObject = BombTypeData.GetAssetByBombType(EBombType.温压弹).BombObject;
                BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.温压弹).PointerVFX;
                break;
            case 1:
                BombObject = BombTypeData.GetAssetByBombType(EBombType.常规弹药打击).BombObject;
                BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.常规弹药打击).PointerVFX;
                break;
            case 2:
                BombObject = BombTypeData.GetAssetByBombType(EBombType.核爆).BombObject;
                BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.核爆).PointerVFX;
                break;
        }

        if (BombPointerVFX)
        {
            SpwanPointerVFX(BombPointerVFX, TargetPosition);
        }

        if (BombObject != null)
        {
            SpawnBomb(BombObject, SpawnPosion, SpwanRotation, TargetPosition, StrikeLevel);
            
            // 构造爆炸源数据并广播
            ExplosiveSourceData data = new ExplosiveSourceData()
            {
                type = BombObject.GetComponentInChildren<Bomb>().BombType.ToString(),
                strike_level = StrikeLevel,
                x_coordinate = TargetPosition.x,
                y_coordinate = TargetPosition.z
            };
            
            ExplosionDataEvent?.Invoke(data);
            MessageBox.Instance.PrintExplosionData(data);
            
            // 创建 DamageData 实例并随机生成损伤百分比
            DamageData damageData = new DamageData();
            damageData.damage_percent_1 = Random.Range(0f, 100f);
            damageData.damage_percent_2 = Random.Range(0f, 100f);
            damageData.damage_percent_3 = Random.Range(0f, 100f);
            damageData.damage_percent_4 = Random.Range(0f, 100f);
            damageData.damage_percent_5 = Random.Range(0f, 100f);

            // 广播损伤数据
            DamageDataEvent?.Invoke(damageData);
            damageDecalManager.OnDamageMessageReceived(damageData);
            MessageBox.Instance.PrintDamageData(damageData);
            UpdateStatusText(data);
            UpdateDistanceText(TargetPosition);
        }
    }

    /// <summary>
    /// 根据WebSocket接收到的数据生成炸弹
    /// </summary>
    /// <param name="data">爆炸源数据</param>
    public void SpawnBombByWebSocket(ExplosiveSourceData data)
    {
        if(BombObjects.Count == 0)
        {
            Debug.LogError("BombObjects 为空");
            return;
        }
        
        string type = data.type;
        int strike_level = (int)data.strike_level;
        float x_coordinate = data.x_coordinate;
        float y_coordinate = data.y_coordinate;

        Vector3 SpawnPosion = SpawnPositions[Random.Range(0, SpawnPositions.Count)];
        Quaternion SpwanRotation = Quaternion.AngleAxis(-80, Vector3.right); // 四元数绕x轴旋转-80度

        Vector3 TargetPosition = new Vector3(x_coordinate, 0, y_coordinate);

        // 根据类型生成对应的炸弹和指示特效
        GameObject BombObject = null;
        GameObject BombPointerVFX = null;
        switch (type)
        {
            case "温压弹":
                BombObject = BombTypeData.GetAssetByBombType(EBombType.温压弹).BombObject;
                BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.温压弹).PointerVFX;
                break;
            case "常规弹药打击":
                BombObject = BombTypeData.GetAssetByBombType(EBombType.常规弹药打击).BombObject;
                BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.常规弹药打击).PointerVFX;
                break;
            case "核爆":
                BombObject = BombTypeData.GetAssetByBombType(EBombType.核爆).BombObject;
                BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.核爆).PointerVFX;
                break;
        }
        
        if (BombPointerVFX)
        {
            SpwanPointerVFX(BombPointerVFX, TargetPosition);
        }

        if (BombObject != null)
        {
            SpawnBomb(BombObject, SpawnPosion, SpwanRotation, TargetPosition, strike_level);
            UpdateDistanceText(TargetPosition);
        }
    }
    
    /// <summary>
    /// 更新距离文本
    /// </summary>
    /// <param name="TargetPosition">目标位置</param>
    private void UpdateDistanceText(Vector3 TargetPosition)
    {
        if (distanceText != null && modelTransform != null)
        {
            float distance = Vector3.Distance(modelTransform.position, TargetPosition);
            distanceText.text = $"爆源距离: {distance:F1}米";
        }
    }
    
    /// <summary>
    /// 生成炸弹
    /// </summary>
    /// <param name="BombObject">炸弹对象</param>
    /// <param name="SpawnPosion">生成位置</param>
    /// <param name="SpwanRotation">生成旋转</param>
    /// <param name="TargetPosition">目标位置</param>
    /// <param name="StrikeLevel">打击等级</param>
    private void SpawnBomb(GameObject BombObject, Vector3 SpawnPosion, Quaternion SpwanRotation, Vector3 TargetPosition, int StrikeLevel)
    {
        // 生成炸弹
        Bomb bomb = Instantiate(BombObject.GetComponentInChildren<Bomb>(), SpawnPosion, SpwanRotation);
        bomb.TargetPosition = TargetPosition;
        bomb.StrikeLevel = StrikeLevel;
        
        // 根据等级设置扫描范围
        if (ScanFX && StrikeLevelData.ShockWaveRange.Count >= 4)
        {
            ScanFX.SetScanRange(StrikeLevelData.ShockWaveRange[StrikeLevel - 1]);
        }
    }
    
    /// <summary>
    /// 生成指示特效
    /// </summary>
    /// <param name="PointerVFX">指示特效对象</param>
    /// <param name="TargetPosition">目标位置</param>
    private void SpwanPointerVFX(GameObject PointerVFX, Vector3 TargetPosition)
    {
        // 竖直向下射出射线
        Ray ray = new Ray(new Vector3(TargetPosition.x, 1000, TargetPosition.z), Vector3.down);
        
        // 在交点位置生成指示特效
        if (Physics.Raycast(ray, out RaycastHit hit, 1500, 1 << LayerMask.NameToLayer("Terrain"), QueryTriggerInteraction.Ignore))
        {
            // 生成指示特效
            GameObject pointerVFX = Instantiate(PointerVFX, hit.point, Quaternion.identity);
            Destroy(pointerVFX, 5.0f);
        }
    }
    
    /// <summary>
    /// 更新状态文本
    /// </summary>
    /// <param name="data">爆炸源数据</param>
    private void UpdateStatusText(ExplosiveSourceData data)
    {
        if (WebSocketConsumer.Instance != null)
        {
            WebSocketConsumer.Instance.UpdateStatusText($"类型: {data.type}\n等级: {data.strike_level}\n坐标: ({data.x_coordinate:F3}, {data.y_coordinate:F3})");
        }
        else
        {
            Debug.LogError("WebSocketConsumer.Instance is null!");
        }
    }
}
