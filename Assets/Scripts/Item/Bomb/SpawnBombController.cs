using System.Collections.Generic;
using INab.WorldScanFX.URP;
using JsonStruct;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using TMPro;

public class SpawnBombController : MonoSingleton<SpawnBombController>
{
    [Header("主变量")]
    public List<GameObject> BombObjects;
    public ScanFX ScanFX;
    public StrikeLevelData StrikeLevelData;
    public BombTypeData BombTypeData;
    
    [Header("测试生成参数")]
    public List<Vector3> SpawnPositions;
    public float TargetRange= 500.0f;
    
    [Header("委托/事件")]
    public UnityEvent<ExplosiveSourceData> ExplosionDataEvent; //用于向UI发送爆源数据的事件
    
    [Header("UI组件")]
    public TextMeshProUGUI distanceText; // 新增显示距离的文本组件
    public Transform modelTransform;  // 用于计算距离的模型 Transform

    protected override void Awake()
    {
        base.Awake(); 
        ScanFX = GetComponent<ScanFX>();
    }
    
    //暂时测试
    public void SpawnBomb()
    {
        if(BombObjects.Count == 0)
        {
            Debug.LogError("BombObjects 为空");
            return;
        }
        
        //暂时随机生成
         int StrikeLevel = Random.Range(1, 5);
         Vector3 SpawnPosion = SpawnPositions[Random.Range(0, SpawnPositions.Count)];
         Quaternion SpwanRotation = Quaternion.AngleAxis(-80, Vector3.right);  //四元数绕x轴旋转-80度
         
         Vector3 TargetPosition = new Vector3(Random.Range(-TargetRange, TargetRange), 0, Random.Range(-TargetRange, TargetRange));
         
         //根据type生成对应类型的炸弹
         GameObject BombObject = null;
         GameObject BombPointerVFX = null;
         
         switch (Random.Range(0,3)) //随机类型
         {
             case 0:
                 BombObject = BombTypeData.GetAssetByBombType(EBombType.温压弹).BombObject;
                 BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.温压弹).PointerVFX;
                 break;
             case 1:
                 BombObject = BombTypeData.GetAssetByBombType(EBombType.堵口爆).BombObject;
                 BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.堵口爆).PointerVFX;
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
             
             //广播
             ExplosiveSourceData data = new ExplosiveSourceData()
             {
                 type = BombObject.GetComponentInChildren<Bomb>().BombType.ToString(),
                 strike_level = StrikeLevel,
                 x_coordinate = TargetPosition.x,
                 y_coordinate = TargetPosition.z
             };
             
             ExplosionDataEvent?.Invoke(data);
             MessageBox.Instance.PrintExplosionData(data);
             // 更新 statusText 文本
             // Debug.Log($"类型: {data.type}\n等级: {data.strike_level}\n坐标: ({data.x_coordinate:F3}, {data.y_coordinate:F3})");
             if (WebSocketConsumer.Instance != null)
             {
                 WebSocketConsumer.Instance.UpdateStatusText($"类型: {data.type}\n等级: {data.strike_level}\n坐标: ({data.x_coordinate:F3}, {data.y_coordinate:F3})");
             }
             else
             {
                 Debug.LogError("WebSocketConsumer.Instance is null!");
             }
             // 计算并显示爆源和模型的距离
             UpdateDistanceText(TargetPosition);
         }
    }

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
        Quaternion SpwanRotation = Quaternion.AngleAxis(-80, Vector3.right); //四元数绕x轴旋转-80度

        Vector3 TargetPosition = new Vector3(x_coordinate, 0, y_coordinate);

        //根据type生成对应类型的 炸弹 和 指示特效
        GameObject BombObject = null;
        GameObject BombPointerVFX = null;
        switch (type) //type
        {
            case "温压弹":
                BombObject = BombTypeData.GetAssetByBombType(EBombType.温压弹).BombObject;
                BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.温压弹).PointerVFX;
                break;
            case "堵口爆":
                BombObject = BombTypeData.GetAssetByBombType(EBombType.堵口爆).BombObject;
                BombPointerVFX = BombTypeData.GetAssetByBombType(EBombType.堵口爆).PointerVFX;
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
            // 计算并显示爆源和模型的距离
            UpdateDistanceText(TargetPosition);
        }
    }
    
    private void UpdateDistanceText(Vector3 TargetPosition)
    {
        if (distanceText != null && modelTransform != null)
        {
            float distance = Vector3.Distance(modelTransform.position, TargetPosition);
            distanceText.text = $"爆源距离: {distance:F1}米";
        }
    }
    
    /// <summary>
    /// 生成炮弹
    /// </summary>
    private void SpawnBomb(GameObject BombObject, Vector3 SpawnPosion, Quaternion SpwanRotation, Vector3 TargetPosition, int StrikeLevel)
    {
        //生成炸弹
        Bomb bomb = Instantiate(BombObject.GetComponentInChildren<Bomb>(), SpawnPosion, SpwanRotation);
        bomb.TargetPosition =  TargetPosition;
        bomb.StrikeLevel = StrikeLevel;
        
        // 根据等级设置扫描范围
        if (ScanFX && StrikeLevelData.ShockWaveRange.Count >= 4)
        {
            ScanFX.SetScanRange(StrikeLevelData.ShockWaveRange[StrikeLevel-1]);
        }
    }
    
    /// <summary>
    /// 生成指示特效
    /// </summary>
    private void SpwanPointerVFX(GameObject PointerVFX, Vector3 TargetPosition)
    {
        //竖直向下射出射线
        Ray ray = new Ray(new Vector3(TargetPosition.x, 1000, TargetPosition.z), Vector3.down);
        
        //在交点位置生成指示特效
        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                1500,
                1 << LayerMask.NameToLayer("Terrain"), 
                QueryTriggerInteraction.Ignore))
        {
            //生成指示特效
            GameObject pointerVFX = Instantiate(PointerVFX, hit.point, Quaternion.identity);
            Destroy(pointerVFX, 5.0f);
        }
    }
    
}