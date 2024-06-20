using System;
using JsonStruct;
using UnityEngine;
using UnityEngine.Events;

public class KafkaListener : MonoSingleton<KafkaListener>
{
    //接收到Kafka消息后广播该事件
    public UnityEvent<ExplosiveSourceData> OnKafkaMessageReceived;
    
    private void Start()
    {
        ExplosiveSourceData data = JsonDataManager.Instance.LoadData<ExplosiveSourceData>("TestData", JsonType.LitJson);
        OnKafkaMessageReceived?.Invoke(data);    
    }
    
}