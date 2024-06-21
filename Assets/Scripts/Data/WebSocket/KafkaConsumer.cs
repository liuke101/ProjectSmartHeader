// using Confluent.Kafka;
// using System.Collections;
// using UnityEngine;
// using System.Collections.Concurrent;
// using System.Text;
// using JsonStruct;
// using LitJson;
// using UnityEngine.Events;
//
// class KafkaConsumer : MonoSingleton<KafkaConsumer>
// {
//     [Header("协程")]
//     Coroutine kafkaCoroutine;
//
//     private IConsumer<Ignore, byte[]> Consumer;
//     
//     ConsumerConfig config;
//     public readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
//     public bool kafkaStarted = false;
//     
//     //接收到爆源数据时进行广播
//     public UnityEvent<ExplosiveSourceData> OnKafkaMessageReceived;
//
//     void Start()
//     {
//         config = new ConsumerConfig
//         {
//             GroupId = "group5", // 你的消费者组ID
//             BootstrapServers = "10.151.1.109:9092", // 你的Kafka集群地址
//             AutoOffsetReset = AutoOffsetReset.Earliest // 自动偏移量重置策略
//         };
//         Debug.Log("Kafka - 创建config");
//         
//         Consumer = new ConsumerBuilder<Ignore, byte[]>(config).Build();
//         Consumer.Subscribe("notify_feature"); // 订阅你的Kafka主题
//         
//         Debug.Log("Kafka - 订阅");
//         
//         StartKafkaCoroutine();
//     }
//     
//     void Update()
//     {
//         if (Input.GetKeyUp(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.C))
//         {
//             Debug.Log("Cancelling Kafka!");
//             StopKafkaCoroutine();
//         }
//
//         ProcessKafkaMessage();
//     }
//
//     void OnDisable()
//     {
//         StopKafkaCoroutine();
//     }
//     void OnApplicationQuit()
//     {
//         StopKafkaCoroutine();
//     }
//     private void OnDestroy()
//     {
//         StopKafkaCoroutine();
//     }
//
//     public void StartKafkaCoroutine()
//     {
//         if (kafkaStarted) return;
//         kafkaCoroutine = StartCoroutine(KafkaCoroutine());
//         kafkaStarted = true;
//     }
//     
//     public void StopKafkaCoroutine()
//     {
//         if (kafkaStarted)
//         {
//             StopCoroutine(kafkaCoroutine);
//             kafkaStarted = false;
//         }
//     }
//     
//     private void ProcessKafkaMessage()
//     {
//         if (kafkaStarted)
//         {
//             string message;
//             while (_queue.TryDequeue(out message))
//             {
//                 //判断message是否包含"explosion"
//                 if (message.Contains("explosion"))
//                 {
//                     Debug.Log(message);
//                     ExplosiveSourceData data = JsonMapper.ToObject<ExplosiveSourceData>(message);
//                     
//                     //广播有效数据
//                     if(data != null)
//                     {
//                         OnKafkaMessageReceived?.Invoke(data);
//                     }
//                 }
//             }
//         }
//     }
//     
//     IEnumerator KafkaCoroutine()
//     {
//         Debug.Log("Kafka - 开始协程");
//         
//         while (true)
//         {
//             // Waiting for message
//             var cr = Consumer.Consume();
//             // Got message! Decode and put on queue
//             string message = Encoding.UTF8.GetString(cr.Message.Value);
//             _queue.Enqueue(message);
//             
//             yield return null; //等待下一帧
//         }
//     }
// }