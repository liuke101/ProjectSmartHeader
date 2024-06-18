using System.Collections.Generic;
using INab.WorldScanFX.URP;
using JsonStruct;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnBombController : MonoSingleton<SpawnBombController>
{
    public List<GameObject> BombObjects;
    public ScanFX ScanFX;

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
        
        // 读取json
        ExplosiveSourceData data = JsonDataManager.Instance.LoadData<ExplosiveSourceData>("TestData");
        
        //读取数据，并转换到对应的类型
        var x_coordinate = data.value.features.x_coordinate.properties.value.ConvertTo<float>();
        var y_coordinate = data.value.features.y_coordinate.properties.value.ConvertTo<float>();
        var type = data.value.features.type.properties.value.ConvertTo<string>();
        var strike_level = data.value.features.strike_level.properties.value.ConvertTo<int>();
        //打印
        //Debug.Log("x_coordinate: " + x_coordinate + "   " +"y_coordinate: " + y_coordinate + "   " +"type: " + type + "   "+ "strike_level: " + strike_level);
        
        //根据strike_level获取对应的BombLevel枚举
         EBombLevel eBombLevel = EBombLevel.ONE;
         switch (Random.Range(1,4)) //strike_level
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
         Debug.Log("爆炸登记: " + eBombLevel);
         
         
         Vector3 SpawnPosion = new Vector3(Random.Range(-200, 200), 100,Random.Range(-200, 200));
         // Vector3 TargetPosition = new Vector3(x_coordinate, 0, y_coordinate);
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
         switch (type)
         {
             case "温压弹":
                 BombObjects.Find(bombobject => bombobject.GetComponentInChildren<Bomb>().EBombType == EBombType.温压弹).GetComponentInChildren<Bomb>().SpawnBomb( SpawnPoint.transform, TargetPoint.transform, eBombLevel);
                 break;
             case "堵口爆":
                 BombObjects.Find(bombobject => bombobject.GetComponentInChildren<Bomb>().EBombType == EBombType.堵口爆).GetComponentInChildren<Bomb>().SpawnBomb( SpawnPoint.transform, TargetPoint.transform, eBombLevel);
                 break;
             case "核弹":
                 BombObjects.Find(bombobject => bombobject.GetComponentInChildren<Bomb>().EBombType == EBombType.核弹).GetComponentInChildren<Bomb>().SpawnBomb( SpawnPoint.transform, TargetPoint.transform, eBombLevel);
                 break;
         }
    }

}