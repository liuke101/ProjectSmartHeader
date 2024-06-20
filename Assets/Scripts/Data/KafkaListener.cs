using System;
using JsonStruct;
using UnityEngine;
using UnityEngine.Events;

public class KafkaListener : MonoSingleton<KafkaListener>
{
    public UnityEvent<ExplosiveSourceData> OnKafkaMessageReceived;
    
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        ExplosiveSourceData data = JsonDataManager.Instance.LoadData<ExplosiveSourceData>("TestData", JsonType.LitJson);
        OnKafkaMessageReceived?.Invoke(data);    
    }
    
}