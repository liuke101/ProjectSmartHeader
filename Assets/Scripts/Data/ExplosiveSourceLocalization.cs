using System.Collections.Generic;
using JsonStruct;
using UnityEngine;

public class ExplosiveSourceLocalization : MonoSingleton<ExplosiveSourceLocalization>
{
    // //消息队列：存储所有发送来的数据，并在处理后删除
    // public Queue<ExplosiveSourceData> ExplosiveSourceDataQueue;
    //
    // void Start()
    // {
    //     //1. 反序列化，将Json字符串转换为类对象
    //     ExplosiveSourceData data = JsonDataManager.Instance.LoadData<ExplosiveSourceData>("TestData", JsonType.LitJson);
    //     //ExplosiveSourceDatas.Enqueue(data);
    //     
    //     //2. 获取data对象的信息
    //     Debug.Log(data.ThingID);
    //     Debug.Log(data.Feature.type);
    //     Debug.Log(data.Feature.strike_level);
    //     Debug.Log(data.Feature.x_coordinate);
    //     Debug.Log(data.Feature.y_coordinate);
    // }

    void Update()
    {
        
    }
}
