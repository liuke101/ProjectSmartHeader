using Confluent.Kafka;
using System;
using System.Threading;
using UnityEngine;
using System.Collections.Concurrent;
using System.Text;
using JsonStruct;
using LitJson;
using UnityEngine.Events;

class KafkaConsumer : MonoSingleton<KafkaConsumer>
{

    [Serializable]
    public class threadHandle
    {
        // IConsumer<Ignore, string> c;
        ConsumerConfig config;

        public readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        public void StartKafkaListener()
        {
            Debug.Log("Kafka - Starting Thread..");
            try
            {
                config = new ConsumerConfig
                {
                    // GroupId = "c#test-consumer-group" + DateTime.Now,  // unique group, so each listener gets all messages
                    // BootstrapServers = "localhost:9092",
                    // AutoOffsetReset = AutoOffsetReset.Earliest
                    GroupId = "group5", // 你的消费者组ID
                    BootstrapServers = "10.151.1.109:9092", // 你的Kafka集群地址
                    AutoOffsetReset = AutoOffsetReset.Earliest // 自动偏移量重置策略
                };

                Debug.Log("Kafka - Created config");

                using var c = new ConsumerBuilder<Ignore, byte[]>(config).Build();
                c.Subscribe("notify_feature"); // 订阅你的Kafka主题
                Debug.Log("Kafka - Subscribed");

                CancellationTokenSource cts = new CancellationTokenSource();
                Console.CancelKeyPress += (_, e) => {
                    e.Cancel = true; // prevent the process from terminating.
                    cts.Cancel();
                };

                try
                {
                    while (true)
                    {
                        try
                        {
                            // Waiting for message
                            var cr = c.Consume(cts.Token);
                            // Got message! Decode and put on queue
                            string message = Encoding.UTF8.GetString(cr.Message.Value);
                            _queue.Enqueue(message);
                        }
                        catch (ConsumeException e)
                        {
                            Debug.Log("Kafka - Error occured: " + e.Error.Reason);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Kafka - Canceled..");
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    c.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Kafka - Received Expection: " + ex.Message + " trace: " + ex.StackTrace);
            }
        }
    }
    public bool kafkaStarted = false;
    Thread kafkaThread;
    threadHandle _handle;
    
    //接收到爆源数据时进行广播
    public UnityEvent<ExplosiveSourceData> OnKafkaMessageReceived;

    void Start()
    {
        StartKafkaThread();
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.C))
        {
            Debug.Log("Cancelling Kafka!");
            StopKafkaThread();
        }

        ProcessKafkaMessage();
    }

    void OnDisable()
    {
        StopKafkaThread();
    }
    
    void OnApplicationQuit()
    {
        StopKafkaThread();
    }

    private void OnDestroy()
    {
        StopKafkaThread();
    }

    public void StartKafkaThread()
    {
        if (kafkaStarted) return;

        _handle = new threadHandle();
        kafkaThread = new Thread(_handle.StartKafkaListener);

        kafkaThread.Start();
        kafkaStarted = true;
        // StartKafkaListener(config);
    }
    private void ProcessKafkaMessage()
    {
        if (kafkaStarted)
        {
            string message;
            while (_handle._queue.TryDequeue(out message))
            {
                //判断message是否包含"explosion"
                if (message.Contains("explosion"))
                {
                    Debug.Log(message);
                    ExplosiveSourceData data = JsonMapper.ToObject<ExplosiveSourceData>(message);
                    
                    //广播有效数据
                    if(data != null)
                    {
                        OnKafkaMessageReceived?.Invoke(data);
                    }
                }
            }
        }
    }

    void StopKafkaThread()
    {
        if (kafkaStarted)
        {
            kafkaThread.Abort();
            kafkaThread.Join();
            kafkaStarted = false;
        }
    }
}