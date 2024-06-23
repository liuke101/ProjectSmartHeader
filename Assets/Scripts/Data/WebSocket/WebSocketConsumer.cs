using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using Best.WebSockets;
using Best.WebSockets.Implementations;
using JsonStruct;
using LitJson;
using UnityEngine.Events;

public class WebSocketConsumer : MonoSingleton<WebSocketConsumer>
{
    [Header("WebSocket")]
    public string URL = "ws://ditto:ditto@10.151.1.109:8080/ws/2";
    public string SubscribeMessage = "START-SEND-EVENTS?filter=eq(thingId,\"edu.whut.cs.iot.se:explosion\")";
    public string SubscribeACK = "START-SEND-EVENTS:ACK";
    private WebSocket WebSocket;
    
    // 消费者队列
    //private readonly ConcurrentQueue<string> CustomerQueue = new ConcurrentQueue<string>();
    
    // 连接协程
    private Coroutine SubscriptionCoroutine;
    
    [Header("事件")]
    // 接收到爆源数据时进行广播
    public UnityEvent<ExplosiveSourceData> OnExplosiveSourceMessageReceived;
    
    private void Start()
    {
        WebSocket = new WebSocket(new Uri(URL));
        
        // 绑定 WebSocket 委托
        if (WebSocket != null)
        {
            WebSocket.OnOpen += OnWebSocketOpen;
            WebSocket.OnMessage += OnMessageReceived; //当接收到消息时调用
            WebSocket.OnClosed += OnWebSocketClosed;
            WebSocket.Open();
        }
    }

    private void Update()
    {
        //ProcessMessage();
    }

    private void OnDestroy()
    {
        WebSocket.Close();
    }

    //开关UI
    public void SwitchState(bool isOpen)
    {
        if (isOpen)
        {
            WebSocket.Open();
        }
        else
        {
            WebSocket.Close();
        }
    }
    
    private void OnWebSocketOpen(WebSocket webSocket) 
    {
        MessageBox.Instance.PrintMessage("WebSocket 开启成功");
        // 开启订阅协程
        SubscriptionCoroutine = StartCoroutine(Subscription());
    }
    
    private void OnMessageReceived(WebSocket webSocket, string message)
    {
        // 如果接收到ACK，停止协程
        if(message == SubscribeACK) 
        {
            MessageBox.Instance.PrintMessage("WebSocket 订阅成功, 关闭协程");
            StopCoroutine(SubscriptionCoroutine);
        }
        // 如果是json爆源数据
        else if(message.Contains("explosion")) 
        {
            //处理数据后入队
            //CustomerQueue.Enqueue(JsonParser(message));
            
            //将json解析为项目所用数据结构
            ExplosiveSourceData data = JsonMapper.ToObject<ExplosiveSourceData>(JsonParser(message));
             
            //广播有效数据
            if(data != null)
            {
                OnExplosiveSourceMessageReceived?.Invoke(data);
                MessageBox.Instance.PrintExplosionData(data);
            }
        }
    }
    
    private void OnWebSocketClosed(WebSocket webSocket, WebSocketStatusCodes code, string message)
    {
        MessageBox.Instance.PrintMessage("WebSocket 正在关闭");
    
        if (code == WebSocketStatusCodes.NormalClosure)
        {
            MessageBox.Instance.PrintMessage("WebSocket 关闭成功");
        }
        else 
        {
            // Error
            MessageBox.Instance.PrintMessage("WebSocket 发生了错误" + code);
        }
    }
    
    // private void ProcessMessage()
    // {
    //     while (CustomerQueue.TryDequeue(out var message))
    //     {
    //          ExplosiveSourceData data = JsonMapper.ToObject<ExplosiveSourceData>(message);
    //          
    //          //广播有效数据
    //          if(data != null)
    //          {
    //              OnExplosiveSourceMessageReceived?.Invoke(data);
    //              MessageBox.Instance.PrintExplosionData(data);
    //          }
    //     }
    // }

    //将json解析为项目所用格式
    private string JsonParser(string message)
    {
        MessageBox.Instance.PrintMessage("解析Json数据");
        
        // 解析 JSON 数据
        JsonData jsonData = JsonMapper.ToObject(message);

        // 提取特定字段
        string type = (string)jsonData["value"]["features"]["type"]["properties"]["value"];
        double strikeLevel = (double)jsonData["value"]["features"]["strike_level"]["properties"]["value"];
        double xCoordinate = (double)jsonData["value"]["features"]["x_coordinate"]["properties"]["value"];
        double yCoordinate = (double)jsonData["value"]["features"]["y_coordinate"]["properties"]["value"];

        // 创建新的 JSON 对象
        JsonData result = new JsonData();
        result["type"] = type;
        result["strike_level"] = strikeLevel;
        result["x_coordinate"] = xCoordinate;
        result["y_coordinate"] = yCoordinate;

        // 输出结果
        string resultJson = result.ToJson();
        return resultJson;
    }
    
    //Send发送订阅等待connecting
    IEnumerator Subscription()
    {
        while (true)
        {
            //如果未连接，持续发送订阅消息
            if (WebSocket.IsOpen)
            {
                WebSocket.Send(SubscribeMessage);
                MessageBox.Instance.PrintMessage("发送 WebSocket 订阅消息");
            }
            
            yield return new WaitForSeconds(1.0f);
        }
    }
}