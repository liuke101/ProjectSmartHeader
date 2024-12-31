using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using Best.WebSockets;
using Best.WebSockets.Implementations;
using JsonStruct;
using LitJson;
using UnityEngine.Events;
using TMPro;

public class WebSocketConsumer : MonoSingleton<WebSocketConsumer>
{
    [Header("WebSocket")]
    public string URL = "ws://ditto:ditto@10.151.1.109:8080/ws/2";
    // 订阅多个 thingId
    public string thingId1 = "edu.whut.cs.iot.se:ship";  // 第一个thingId
    public string thingId2 = "edu.whut.cs.iot.se:construction";  // 第二个thingId
    private string SubscribeMessage => 
        $"START-SEND-EVENTS?filter=eq(thingId,\"{thingId1}\") OR eq(thingId,\"{thingId2}\")";  // 使用 OR 连接多个thingId

    public string SubscribeACK = "START-SEND-EVENTS:ACK";
    private WebSocket WebSocket;
    
    public TextMeshProUGUI distanceText;  // 新增用于显示距离的文本组件
    public Transform modelTransform;  // 用于计算与爆源的距离的模型的Transform
    
    public GameObject statusTextParent;     // statusText 的父对象
    private Coroutine displayCoroutine;
    // TextMeshPro UI 元素
    public TextMeshProUGUI statusText;
    // 定义一个确定的目标坐标
    Vector3 targetPosition = new Vector3(0f, 175f, 0f);
    // 更新 statusText 的方法
    // 五个 GameObject 用于显示 damage_percent 的不同值
    public GameObject damageObject1;
    public GameObject damageObject2;
    public GameObject damageObject3;
    public GameObject damageObject4;
    public GameObject damageObject5;

    // 用于计算颜色的区间
    private readonly Color greenColor = Color.green;
    private readonly Color yellowColor = Color.yellow;
    private readonly Color redColor = Color.red;

    public void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            // Debug.Log("statusText updated with message: " + message);
            // 如果有正在进行的协程，先停止它
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }

            // 启动新的协程来显示父对象5秒
            displayCoroutine = StartCoroutine(DisplayStatusTextForSeconds(4,5));
        }
        else
        {
            Debug.LogError("statusText is not assigned in WebSocketConsumer!");
        }
    }
    // 协程：显示父对象并等待指定时间后隐藏
    private IEnumerator DisplayStatusTextForSeconds(float delay, float displayDuration)
    {
        // 先等待指定的延迟时间
        yield return new WaitForSeconds(delay);

        // 显示父对象
        RectTransform rectTransform = statusTextParent.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 设置目标位置（注意：这里的坐标是相对于父物体的）
            rectTransform.anchoredPosition = targetPosition;
        }
        statusTextParent.SetActive(true);

        // 再等待显示时间
        yield return new WaitForSeconds(displayDuration);

        // 隐藏父对象
        statusTextParent.SetActive(false);
    }
    
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
        // 如果是json损伤百分比数据
        else if (message.Contains("damage_percent"))
        {
            // 使用 JsonParserDamage 解析 damage_percent 数据
            DamageData damageData = JsonParserDamage(message);
            if (damageData != null)
            {
                // 更新 damage_percent 对应的颜色
                UpdateComponentColors(damageData);
            }
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
                // 计算爆源和模型的距离
                Vector3 explosionPosition = new Vector3(data.x_coordinate, 0, data.y_coordinate);
                float distance = Vector3.Distance(modelTransform.position, explosionPosition);

                // 将距离显示在distanceText上
                distanceText.text = $"爆源距离: {distance:F1}米";
                // 显示爆源数据到 Text UI
                // statusText.text = "nihao";
                Instance.UpdateStatusText($"类型: {data.type}\n等级: {data.strike_level}\n坐标: ({data.x_coordinate:F3}, {data.y_coordinate:F3})");
                
            }
        }
    }
    
    // 将损伤百分比数据解析为需要的格式
    private void UpdateComponentColors(DamageData data)
    {
        // 根据 damage_percent 的值设置颜色
        UpdateColorForComponent(damageObject1, data.damage_percent_1);
        UpdateColorForComponent(damageObject2, data.damage_percent_2);
        UpdateColorForComponent(damageObject3, data.damage_percent_3);
        UpdateColorForComponent(damageObject4, data.damage_percent_4);
        UpdateColorForComponent(damageObject5, data.damage_percent_5);
    }

    // 根据 damage_percent 值更新颜色
    private void UpdateColorForComponent(GameObject component, float damagePercent)
    {
        // 获取该 GameObject 的 Renderer 组件
        Renderer renderer = component.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 根据 damage_percent 划分颜色区间
            Color color = Color.white;  // 默认白色
            if (damagePercent <= 30)
            {
                color = greenColor;  // 0-30 区间，绿色
            }
            else if (damagePercent <= 70)
            {
                color = yellowColor;  // 30-70 区间，黄色
            }
            else
            {
                color = redColor;  // 70 以上，红色
            }

            // 设置物体颜色
            renderer.material.color = color;
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
    
    // 新增的 JsonParserDamage 用于解析 damage_percent 数据
    private DamageData JsonParserDamage(string message)
    {
        MessageBox.Instance.PrintMessage("解析 damage_percent Json数据");

        JsonData jsonData = JsonMapper.ToObject(message);

        // 提取 damage_percent 数据
        float damagePercent1 = (float)jsonData["value"]["features"]["damage_percent_1"]["properties"]["value"];
        float damagePercent2 = (float)jsonData["value"]["features"]["damage_percent_2"]["properties"]["value"];
        float damagePercent3 = (float)jsonData["value"]["features"]["damage_percent_3"]["properties"]["value"];
        float damagePercent4 = (float)jsonData["value"]["features"]["damage_percent_4"]["properties"]["value"];
        float damagePercent5 = (float)jsonData["value"]["features"]["damage_percent_5"]["properties"]["value"];

        // 创建并返回 DamagePercentData 数据结构
        DamageData result = new DamageData()
        {
            damage_percent_1 = damagePercent1,
            damage_percent_2 = damagePercent2,
            damage_percent_3 = damagePercent3,
            damage_percent_4 = damagePercent4,
            damage_percent_5 = damagePercent5
        };

        return result;
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
