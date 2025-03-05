﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using UnityEngine;
using Best.WebSockets;
using Best.WebSockets.Implementations;
using JsonStruct;
using LitJson;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// WebSocket连接管理类，负责处理传感器数据的订阅、接收与分发
/// 继承自MonoSingleton实现单例模式
/// </summary>
public class SensorWebsocket : MonoSingleton<SensorWebsocket>
{
    // region WebSocket配置参数
    [Header("WebSocket")]
    /// <summary>WebSocket服务端连接地址</summary>
    public string URL = "ws://ditto:ditto@localhost:8080/ws/2";
    // /// <summary>订阅消息模板，包含设备过滤条件</summary>
    // public string SubscribeMessage = $"START-SEND-EVENTS?filter=eq(thingId,\"edu.whut.cs.iot.se:{sensorType}\")";
    /// <summary>服务端返回的成功订阅确认标识</summary>
    private string SubscribeACK = "START-SEND-EVENTS:ACK";
    
    private string sensorType;
    private string channelNumber;
    
    /// <summary>WebSocket客户端实例</summary>
    private WebSocket WebSocket;
    /// <summary>订阅消息发送协程控制器</summary>
    private Coroutine SubscriptionCoroutine;

    // region 组件引用与事件

    /// <summary>损伤贴图管理组件引用</summary>
    public SensorManager SensorManager;

   // public SensorClickHandler SensorClickHandler;
    // [Header("事件")]
    /// <summary>当接收到损伤数据时触发的事件，携带DamageData参数</summary>
    // public UnityEvent<SensorData> OnSensorMessageReceived;

    /// <summary>
    /// 初始化WebSocket连接并注册事件回调
    /// </summary>
    private void Start()
    {
        // FindObjectOfType<SensorClickHandler>().OnInteract();
        // // 查找Handler并订阅事件
        // var handler = FindObjectOfType<SensorClickHandler>();
        // if (handler != null)
        // {
        //     handler.OnSensorSelected += OnSensorSelected;
        // }
        // else
        // {
        //     Debug.LogError("未找到SensorClickHandler实例！");
        // }
        // WebSocket客户端初始化
        WebSocket = new WebSocket(new Uri(URL));
        
        // 绑定WebSocket事件处理器
        WebSocket.OnOpen += OnWebSocketOpen;
        WebSocket.OnMessage += OnMessageReceived;
        WebSocket.OnClosed += OnWebSocketClosed;
        
        WebSocket.Open();
    }
    // 缓存当前活动的SensorClickHandler
    // private SensorClickHandler _currentHandler;

    // public void ClearHandler(SensorClickHandler handler)
    // {
    //     if (_currentHandler == handler)
    //     {
    //         _currentHandler = null;
    //     }
    // }
    // private void Update()
    // {
    //     // 仅当参数需要更新时查找（如首次初始化或Handler被销毁）
    //     if (_currentHandler == null)
    //     {
    //         _currentHandler = FindObjectOfType<SensorClickHandler>();
    //         if (_currentHandler != null)
    //         {
    //             _currentHandler.OnInteract();
    //             Debug.Log($"sensorType：{sensorType}, 通道 {channelNumber}");
    //         }
    //     }
    // }

    // private void OnSensorSelected(string thingId, string channelNumber)
    // {
    //     // 动态更新参数
    //     sensorType = thingId;
    //     this.channelNumber = channelNumber;
    //     Debug.Log($"更新传感器参数: {sensorType}, 通道 {channelNumber}");
    // }
    public void CurrentSensor(string thingId, string channelNumber)
    {
        sensorType = thingId;
        this.channelNumber = channelNumber;
    }

    /// <summary>
    /// 对象销毁时关闭WebSocket连接
    /// </summary>
    private void OnDestroy()
    {
        WebSocket.Close();
    }

    /// <summary>
    /// 控制WebSocket连接状态
    /// </summary>
    /// <param name="isOpen">true打开连接，false关闭连接</param>
    public void SwitchState(bool isOpen)
    {
        if (isOpen) WebSocket.Open();
        else WebSocket.Close();
    }

    /// <summary>
    /// WebSocket连接成功回调
    /// </summary>
    /// <param name="webSocket">触发事件的WebSocket实例</param>
    private void OnWebSocketOpen(WebSocket webSocket) 
    {
        Debug.Log("传感器订阅开启成功");
        SubscriptionCoroutine = StartCoroutine(Subscription());
    }

    /// <summary>
    /// 处理接收到的WebSocket消息
    /// </summary>
    /// <param name="webSocket">触发事件的WebSocket实例</param>
    /// <param name="message">接收到的消息内容</param>
    private void OnMessageReceived(WebSocket webSocket, string message)
    {
        // Debug.Log($"收到原始消息: {message}"); // DEBUG: 显示原始数据

        // if(message == SubscribeACK) 
        // {
        //     Debug.Log("收到订阅确认ACK，停止协程");
        //     StopCoroutine(SubscriptionCoroutine);
        // }
        // else 
        if(message.Contains(sensorType)) 
        {
            // Debug.Log($"检测到{sensorType}传感器数据消息");
            Debug.Log($"当前传感器通道：{channelNumber}");
            try 
            {
                string parsedJson = JsonParser(message);
                // Debug.Log($"解析后的JSON: {parsedJson}");
            
                SensorData sensorData = JsonMapper.ToObject<SensorData>(parsedJson);
                if (sensorData != null)
                {
                    float channelValue = GetValueByChannel(sensorData, channelNumber);
                    // Debug.Log($"解析成功！通道数据: {channelValue}");
                    SensorManager.UpdatePopupContent(channelValue);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"JSON解析异常: {e.Message}");
            }
        }
    }

    /// <summary>
    /// WebSocket连接关闭回调
    /// </summary>
    /// <param name="webSocket">触发事件的WebSocket实例</param>
    /// <param name="code">关闭状态码</param>
    /// <param name="message">关闭描述信息</param>
    private void OnWebSocketClosed(WebSocket webSocket, WebSocketStatusCodes code, string message)
    {
        if (code == WebSocketStatusCodes.NormalClosure)
            Debug.Log("传感器订阅关闭成功");
        else
            Debug.LogError($"传感器订阅WebSocket异常关闭，错误码：{code}");
    }
    
    public float GetValueByChannel(SensorData data, string channelNumber)
    {
        // 获取SensorData类型信息
        Type type = typeof(SensorData);
        
        // 根据属性名获取PropertyInfo（注意属性是区分大小写的）
        PropertyInfo prop = type.GetProperty(channelNumber);
        
        if (prop == null)
        {
            throw new ArgumentException($"属性 '{channelNumber}' 不存在于SensorData类中。");
        }
        
        // 确保属性类型是float
        if (prop.PropertyType != typeof(float))
        {
            throw new InvalidOperationException($"属性 '{channelNumber}' 不是float类型。");
        }
        
        // 获取属性值
        return (float)prop.GetValue(data);
    }

    /// <summary>
    /// JSON数据解析器，提取传感器数据
    /// </summary>
    /// <param name="message">原始JSON字符串</param>
    /// <returns>格式化后的损伤百分比JSON数据</returns>
    private string JsonParser(string message)
    {
        try 
        {
            JsonData jsonData = JsonMapper.ToObject(message);
            // Debug.Log($"原始JSON结构:\n{jsonData.ToJson()}"); // DEBUG: 输出完整JSON结构

            // DEBUG: 检查关键路径是否存在
            if (!jsonData.Keys.Contains("value")) Debug.LogError("JSON缺少'value'字段");
            if (!jsonData["value"].Keys.Contains("features")) Debug.LogError("JSON缺少'features'字段");

            double channelData = (double)jsonData["value"]["features"][channelNumber]["properties"]["value"];
            // Debug.Log($"提取通道{channelNumber}数据: {channelData}");

            return $"{{\"{channelNumber}\":{channelData}}}";
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON解析失败: {e.Message}\nStack Trace: {e.StackTrace}");
            return "{}";
        }
    }

    /// <summary>
    /// 订阅消息发送协程，每秒发送订阅请求直到收到ACK确认
    /// </summary>
    IEnumerator Subscription()
    {
        while (true)
        {
            if (WebSocket.IsOpen)
            {
                string SubscribeMessage = $"START-SEND-EVENTS?filter=eq(thingId,\"edu.whut.cs.iot.se:{sensorType}\")"; 
                WebSocket.Send(SubscribeMessage);
                // Debug.Log("发送传感器订阅消息");
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}
