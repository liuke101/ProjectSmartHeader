using System;
using UnityEngine;

/// <summary>
/// 处理传感器对象的点击交互事件，负责显示信息弹窗和WebSocket数据订阅
/// </summary>
public class SensorClickHandler : MonoBehaviour
{
    public string sensorName = "光纤光栅加速度传感器";
    public string sensorType = "dhdas";  // 电信号或光信号
    public string sensorNumber = "channel1";
    public string sensorData = "0.0";  // 默认传感器数据

    private SensorManager sensorManager;
    private SensorWebsocket sensorWebSocket;

    /// <summary>
    /// 初始化组件引用，获取场景中的SensorManager和SensorWebSocket实例
    /// </summary>
    void Start()
    {
        
        sensorManager = FindObjectOfType<SensorManager>();  // 获取 sensorManager 组件
        sensorWebSocket = FindObjectOfType<SensorWebsocket>();  // 获取 SensorWebSocket 组件
    }
    

    // public event Action<string, string> OnSensorSelected;

    // 当需要传递参数时（如按钮点击）
    // public void NotifySensorSelected(string thingId, string channelNumber)
    // {
    //     OnSensorSelected?.Invoke(thingId, channelNumber);
    // }
    /// <summary>
    /// 交互事件入口，传递双标识参数到WebSocket系统
    /// </summary>
    public void OnInteract()
    {
        // 传递双标识参数
        SensorWebsocket.Instance.CurrentSensor(
            sensorType, 
            sensorNumber
        );
        // 触发事件
        // OnSensorSelected?.Invoke(sensorType, sensorNumber);
    }

    /// <summary>
    /// 鼠标点击事件处理：显示信息弹窗并建立WebSocket订阅
    /// 包含两个主要操作：
    /// 1. 调用SensorManager显示信息弹窗
    /// 2. 构造订阅消息并发送到WebSocket
    /// </summary>
    void OnMouseDown()
    {
        OnInteract();
        // NotifySensorSelected(sensorType, sensorNumber);
        // Debug.Log($"数据传送成功{sensorType},{sensorNumber}");
        if (sensorManager != null)
        {
            // Debug.Log("正在调用SensorManager显示弹窗...");
            sensorManager.ShowPopup(sensorName, sensorType, sensorData);
        }
        else
        {
            Debug.LogError("SensorManager引用丢失！");
        }

        if (sensorWebSocket != null)
        {
            sensorWebSocket.SwitchState(true);
        }
        else
        {
            Debug.LogError("SensorWebSocket引用丢失！");
        }
    }

    // private void OnDestroy()
    // {
    //     // 通知WebSocket清理缓存
    //     SensorWebsocket.Instance?.ClearHandler(this);
    // }
}
