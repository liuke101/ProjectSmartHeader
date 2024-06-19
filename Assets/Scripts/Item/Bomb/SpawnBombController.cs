using System;
using System.Collections.Generic;
using INab.WorldScanFX.URP;
using JsonStruct;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SpawnBombController : MonoSingleton<SpawnBombController>
{
    public List<GameObject> BombObjects;
    public ScanFX ScanFX;

    protected override void Awake()
    {
        base.Awake(); 
        ScanFX = GetComponent<ScanFX>();
    }

    private void Start()
    {
        SpawnBomb();
    }

    //暂时测试
    public void SpawnBomb()
    {
        if(BombObjects.Count == 0)
        {
            Debug.LogError("BombObjects 为空");
            return;
        }
        
        // 读取json
        ExplosiveSourceData data = JsonDataManager.Instance.LoadData<ExplosiveSourceData>("TestData");
        
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
        
        //打印
        Debug.Log("x_coordinate: " + x_coordinate + "   " +"y_coordinate: " + y_coordinate + "   " +"type: " + type + "   "+ "strike_level: " + strike_level);
        
        //根据strike_level获取对应的BombLevel枚举
         EBombLevel eBombLevel = EBombLevel.ONE;
         switch (Random.Range(1,5)) //strike_level，注意是左闭右开[1,5)
         {
             case 1:
                 eBombLevel = EBombLevel.ONE;
                 break;
             case 2:
                 eBombLevel = EBombLevel.TWO;
                 break;
             case 3:
                 eBombLevel = EBombLevel.THREE;
                 break;
             case 4:
                 eBombLevel = EBombLevel.FOUR;
                 break;
         }
         Debug.Log("爆炸等级: " + eBombLevel);
         
         
         Vector3 SpawnPosion = new Vector3(Random.Range(-200, 200), 100,Random.Range(-200, 200));
         Vector3 TargetPosition = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));
         //在生成位置生成一个空物体，用于确定发射角度和位置
         GameObject SpawnPoint = new GameObject();
         SpawnPoint.transform.position = SpawnPosion;
         SpawnPoint.transform.rotation = Quaternion.Euler(-60, 0, 0); //上斜60度
         //在目标位置生成一个空物体，用于定位
         GameObject TargetPoint = new GameObject();
         TargetPoint.transform.position = TargetPosition;
         
         // TODO:在Bomb类中用委托进行销毁，跟随炸弹销毁
         // Destroy(SpawnPoint);
         // Destroy(TargetPoint);
         
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
             SpawnBomb(BombObject, SpawnPoint.transform, TargetPoint.transform, eBombLevel);
         }
         
    }
    
    public virtual void SpawnBomb(GameObject BombObject, Transform SpawnPointTransform, Transform TargetPointTransfrom, EBombLevel level)
    {
        //生成炸弹
        Bomb bomb = Instantiate(BombObject.GetComponentInChildren<Bomb>(), SpawnPointTransform.position, SpawnPointTransform.rotation);
        bomb.TargetPointTransform = TargetPointTransfrom;
        
        //根据等级设置扫描范围
        //BUG:无效
        if (ScanFX)
        {
            switch (level)
            {
                case EBombLevel.ONE:
                    ScanFX.MaskRadius = 50.0f;
                    break;
                case EBombLevel.TWO:
                    ScanFX.MaskRadius = 100.0f;
                    break;
                case EBombLevel.THREE:
                    ScanFX.MaskRadius = 200.0f;
                    break;
                case EBombLevel.FOUR:
                    ScanFX.MaskRadius = 500.0f;
                    break;
            }
        }
    }

}