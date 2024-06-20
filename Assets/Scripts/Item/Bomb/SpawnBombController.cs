using System.Collections.Generic;
using INab.WorldScanFX.URP;
using JsonStruct;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class SpawnBombController : MonoSingleton<SpawnBombController>
{
    public List<GameObject> BombObjects;
    public ScanFX ScanFX;

    [Header("测试生成参数")]
    public float SpawnRange = 1000.0f;
    public float SpwanHeight= 1000.0f;
    public float TargetRange= 500.0f;
    
    [Header("委托/事件")]
    public UnityEvent<ExplosionData> ExplosionDataEvent; //用于向UI发送爆源数据的事件

    protected override void Awake()
    {
        base.Awake(); 
        ScanFX = GetComponent<ScanFX>();
    }

    private void Start()
    {
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
         Vector3 SpawnPosion = new Vector3(Random.Range(-SpawnRange, SpawnRange), SpwanHeight,Random.Range(-SpawnRange, SpawnRange));
         Quaternion SpwanRotation = Quaternion.AngleAxis(-80, Vector3.right);  //四元数绕x轴旋转-80度
         
         Vector3 TargetPosition = new Vector3(Random.Range(-TargetRange, TargetRange), 0, Random.Range(-TargetRange, TargetRange));
         
         //根据type生成对应类型的炸弹
         GameObject BombObject = null;
         switch (Random.Range(0,3)) //type
         {
             case 0:
                 BombObject = BombObjects.Find(bombobject =>
                     bombobject.GetComponentInChildren<Bomb>().EBombType == EBombType.温压弹);
                 break;
             case 1:
                 BombObject = BombObjects.Find(bombobject =>
                     bombobject.GetComponentInChildren<Bomb>().EBombType == EBombType.堵口爆);
                 break;
             case 2:
                 BombObject = BombObjects.Find(bombobject =>
                     bombobject.GetComponentInChildren<Bomb>().EBombType == EBombType.核弹);
                 break;
         }

         if (BombObject != null)
         {
             SpawnBomb(BombObject, SpawnPosion, SpwanRotation, TargetPosition, StrikeLevel);
         }
    }
    
    /// <summary>
    /// 生成炮弹
    /// </summary>
    /// <param name="BombObject"></param>
    /// <param name="SpawnPosion"></param>
    /// <param name="SpwanRotation">控制炮弹发射角度</param>
    /// <param name="TargetPosition"></param>
    /// <param name="StrikeLevel"></param>
    public virtual void SpawnBomb(GameObject BombObject, Vector3 SpawnPosion, Quaternion SpwanRotation, Vector3 TargetPosition, int StrikeLevel)
    {
        //生成炸弹
        Bomb bomb = Instantiate(BombObject.GetComponentInChildren<Bomb>(), SpawnPosion, SpwanRotation);
        bomb.TargetPosition =  TargetPosition;
        
        //广播
        ExplosionDataEvent?.Invoke(new ExplosionData()
        {
            BombType = bomb.EBombType,
            StrikeLevel = StrikeLevel,
            X_Coordinate = TargetPosition.x,
            Y_Coordinate = TargetPosition.z
        });
        
        
        // 根据等级设置扫描范围
        // BUG:无效
         if (ScanFX)
         {
             switch (StrikeLevel)
             {
                 case 1:
                     ScanFX.MaskRadius = 50.0f;
                     break;
                 case 2:
                     ScanFX.MaskRadius = 100.0f;
                     break;
                 case 3:
                     ScanFX.MaskRadius = 200.0f;
                     break;
                 case 4:
                     ScanFX.MaskRadius = 500.0f;
                     break;
             }
         }
    }

    public void SpawnBombTest(ExplosiveSourceData data)
    {
        if(BombObjects.Count == 0)
        {
            Debug.LogError("BombObjects 为空");
            return;
        }
        
        //读取数据，并转换到对应的类型(必须这样转换，不能直接强转！）
        double x_coordinate = 0;
        if (data.value.features.x_coordinate.properties.value is double x)
        {
            x_coordinate = x;
        }

        double y_coordinate = 0;
        if (data.value.features.y_coordinate.properties.value is double y)
        {
            y_coordinate = y;
        }

        string type = "";
        if (data.value.features.type.properties.value is string t)
        {
            type = t;
        }

        int strike_level = 0;
        if (data.value.features.strike_level.properties.value is int s)
        {
            strike_level = s;
        }

        print("x_coordinate: " + x_coordinate + " y_coordinate: " + y_coordinate + " type: " + type +
              " strike_level: " + strike_level);

        int StrikeLevel = strike_level;

        Vector3 SpawnPosion = new Vector3(Random.Range(-SpawnRange, SpawnRange), SpwanHeight,
            Random.Range(-SpawnRange, SpawnRange));
        Quaternion SpwanRotation = Quaternion.AngleAxis(-80, Vector3.right); //四元数绕x轴旋转-80度

        Vector3 TargetPosition = new Vector3((float)x_coordinate, 0, (float)y_coordinate);

        //根据type生成对应类型的炸弹
        GameObject BombObject = null;
        switch (type) //type
        {
            case "温压弹":
                BombObject = BombObjects.Find(bombobject =>
                    bombobject.GetComponentInChildren<Bomb>().EBombType == EBombType.温压弹);
                break;
            case "堵口爆":
                BombObject = BombObjects.Find(bombobject =>
                    bombobject.GetComponentInChildren<Bomb>().EBombType == EBombType.堵口爆);
                break;
            case "核弹":
                BombObject = BombObjects.Find(bombobject =>
                    bombobject.GetComponentInChildren<Bomb>().EBombType == EBombType.核弹);
                break;
        }

        if (BombObject != null)
        {
            SpawnBomb(BombObject, SpawnPosion, SpwanRotation, TargetPosition, StrikeLevel);
        }
    }
}