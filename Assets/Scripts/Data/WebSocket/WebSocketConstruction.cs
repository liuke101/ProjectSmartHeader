﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;
using Best.WebSockets;
using Best.WebSockets.Implementations;
using JsonStruct;
using LitJson;
using UnityEngine.Events;
using TMPro;

public class WebSocketConstruction : MonoSingleton<WebSocketConstruction>
{
    [Header("WebSocket")]
    public string URL = "ws://ditto:ditto@localhost:8080/ws/2";
    public string SubscribeMessage = "START-SEND-EVENTS?filter=eq(thingId,\"edu.whut.cs.iot.se:construction\")";
    public string SubscribeACK = "START-SEND-EVENTS:ACK";
    private WebSocket WebSocket;
    
    // 连接协程
    private Coroutine SubscriptionCoroutine;
    
    public DamageDecalManager damageDecalManager;  // 引用 DamageDecalManager
    
    [Header("事件")]
    // 接收到爆源数据时进行广播
    public UnityEvent<DamageData> OnDamageMessageReceived;
    
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
        MessageBox.Instance.PrintMessage("结构损伤订阅开启成功");
        // 开启订阅协程
        SubscriptionCoroutine = StartCoroutine(Subscription());
    }
    
    private void OnMessageReceived(WebSocket webSocket, string message)
    {
        // 如果接收到ACK，停止协程
        if(message == SubscribeACK) 
        {
            MessageBox.Instance.PrintMessage("结构损伤数据订阅成功, 关闭协程");
            StopCoroutine(SubscriptionCoroutine);
        }
        // 如果是json损伤数据
        else if(message.Contains("construction")) 
        {
            // MessageBox.Instance.PrintMessage("结构损伤数据订阅成功");
            DamageData damageData = JsonMapper.ToObject<DamageData>(JsonParser(message));
            if (damageData != null)
            {
                Debug.Log("结构损伤数据接收并广播...");
                // 广播结构损伤数据
                OnDamageMessageReceived?.Invoke(damageData);
                MessageBox.Instance.PrintDamageData(damageData);
                damageDecalManager.OnDamageMessageReceived(damageData);
                
            }
        }
    }
    
    private void OnWebSocketClosed(WebSocket webSocket, WebSocketStatusCodes code, string message)
    {
        MessageBox.Instance.PrintMessage("结构损伤订阅正在关闭");
    
        if (code == WebSocketStatusCodes.NormalClosure)
        {
            MessageBox.Instance.PrintMessage("结构损伤订阅关闭成功");
        }
        else 
        {
            // Error
            MessageBox.Instance.PrintMessage("结构损伤订阅WebSocket 发生了错误" + code);
        }
    }

    //将json解析为项目所用格式
    private string JsonParser(string message)
    {
        MessageBox.Instance.PrintMessage("解析结构损伤数据");
        
        // 解析 JSON 数据
        JsonData jsonData = JsonMapper.ToObject(message);

        // 提取 damage_percent 数据
        double damagePercent1 = (double)jsonData["value"]["features"]["damage_percent_1"]["properties"]["value"];
        double damagePercent2 = (double)jsonData["value"]["features"]["damage_percent_2"]["properties"]["value"];
        double damagePercent3 = (double)jsonData["value"]["features"]["damage_percent_3"]["properties"]["value"];
        double damagePercent4 = (double)jsonData["value"]["features"]["damage_percent_4"]["properties"]["value"];
        double damagePercent5 = (double)jsonData["value"]["features"]["damage_percent_5"]["properties"]["value"];

        // 创建并返回 DamagePercentData 数据结构
        JsonData result = new JsonData();
        result["damage_percent_1"] = damagePercent1;
        result["damage_percent_2"] = damagePercent2;
        result["damage_percent_3"] = damagePercent3;
        result["damage_percent_4"] = damagePercent4;
        result["damage_percent_5"] = damagePercent5;

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
                MessageBox.Instance.PrintMessage("发送结构损伤订阅消息");
            }
            
            yield return new WaitForSeconds(1.0f);
        }
    }
}