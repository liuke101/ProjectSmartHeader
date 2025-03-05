using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonStruct;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
/// <summary>
/// 传感器弹窗管理组件，负责创建/更新/销毁传感器数据显示弹窗
/// 要求挂载在包含Canvas的GameObject上，依赖Button组件实现点击关闭功能
/// </summary>
/// <remarks>
/// 功能特性：
/// - 动态生成带标题和数值的悬浮弹窗
/// - 支持世界坐标转UI坐标定位
/// - 数值超过阈值时变色警示
/// - 点击弹窗背景关闭对应弹窗
/// </remarks>
*/
[RequireComponent(typeof(Button))]
public class SensorManager : MonoBehaviour
{
    [Header("UI配置")]
    public GameObject popupPrefab;
    // public Color normalColor = Color.green;
    // public Color warningColor = Color.red;
    // [Range(0, 5)] public float warningThreshold = 2.5f;

    private Dictionary<string, PopupWindow> _activePopups = new Dictionary<string, PopupWindow>();

    private GameObject currentPopup;  // 当前弹窗实例
    private SensorWebsocket sensorWebsocket;
    /// <summary>
    /// 弹窗窗口数据容器类
    /// </summary>
    class PopupWindow
    {
        public GameObject Root;    // 弹窗根物体
        public TMP_Text Title;     // 标题文本组件
        public TMP_Text Value;     // 数值显示文本组件
        public Image Background;   // 背景图片组件（用于点击检测和颜色变化）
    }

    void Start()
    {
        sensorWebsocket = FindObjectOfType<SensorWebsocket>();  // 获取 SensorWebSocket 实例
    }

    public void ShowPopup(string sensorName, string sensorType, string sensorData)
    {
        // DEBUG: 检查预制件引用
        if (popupPrefab == null)
        {
            Debug.LogError("弹窗预制件未赋值！请检查Inspector中的popupPrefab字段");
            return;
        }

        if (currentPopup != null)
        {
            // Debug.Log("检测到已有弹窗，正在销毁旧实例...");
            ClosePopup();
        }

        // DEBUG: Canvas查找验证
        GameObject canvasObj = GameObject.Find("Canvas_HUD");
        if (canvasObj == null)
        {
            Debug.LogError("未找到名称为'Canvas_HUD'的Canvas对象");
            canvasObj = GameObject.Find("Canvas");
            if (canvasObj == null) Debug.LogError("未找到任何Canvas对象");
            else Debug.Log("已找到备用Canvas: " + canvasObj.name);
        }
        else
        {
            // Debug.Log("已找到主Canvas: " + canvasObj.name);
        }

        currentPopup = Instantiate(popupPrefab, Vector3.zero, Quaternion.identity);
        // DEBUG: 确认实例化状态
        if (currentPopup == null)
        {
            Debug.LogError("弹窗实例化失败！");
            return;
        }
        // Debug.Log($"弹窗实例化成功，名称: {currentPopup.name}");

        currentPopup.transform.SetParent(canvasObj.transform, false);
        // DEBUG: 确认父级设置
        // Debug.Log($"弹窗父级设置为: {currentPopup.transform.parent.name}");

        
        Transform content = currentPopup.transform.Find("Content");
        if (content == null)
        {
            Debug.LogError($"弹窗预制件结构错误！未找到Content对象 | 弹窗名称: {currentPopup.name}");
            return;
        }

        TMP_Text titleText = content.Find("Title")?.GetComponent<TMP_Text>();
        TMP_Text valueText = content.Find("Value")?.GetComponent<TMP_Text>();

        // if (titleText == null)
        //     Debug.LogError($"Title文本组件缺失 | Content子对象列表: {GetChildNames(content)}");
        //
        // if (valueText == null)
        //     Debug.LogError($"Value文本组件缺失 | Content子对象列表: {GetChildNames(content)}");

        titleText.text = $"{sensorName}";
        valueText.text = sensorData;
        currentPopup.SetActive(true);
        // Debug.Log($"弹窗内容已更新 | 标题: {titleText.text} | 内容: {valueText.text}");
        // 添加临时标签用于追踪
        // currentPopup.tag = "SensorPopup"; 
    }
   
    // // 辅助方法：获取子对象名称列表
    // private string GetChildNames(Transform parent)
    // {
    //     return string.Join(", ", parent.Cast<Transform>().Select(t => t.name));
    // }


    
    // WebSocket 数据更新后更新弹窗内容
    public void UpdatePopupContent(float sensorData)
    {
        SensorManager manager = FindObjectOfType<SensorManager>();
        GameObject _currentPopup = manager.GetCurrentPopup();
        // Debug.Log($"正在更新弹窗内容，当前弹窗状态: {_currentPopup != null}");
    
        if (_currentPopup != null)
        {
            Transform content = _currentPopup.transform.Find("Content");
            if (content == null)
            {
                Debug.LogError($"弹窗预制件结构错误！未找到Content对象 | 弹窗名称: {currentPopup.name}");
                return;
            }
            TMP_Text contentText = content.Find("Value")?.GetComponent<TMP_Text>();
            if (contentText == null)
            {
                Debug.LogError("未找到Value文本组件");
                return;
            }
            string sensorDataText = $"{sensorData}";
            // Debug.Log($"新数据: {sensorDataText}");
            contentText.text = sensorDataText;
        }
        else
        {
            Debug.LogWarning("尝试更新弹窗内容时，弹窗实例不存在");
        }
    }
    // 弹窗状态验证方法
    public bool IsPopupAlive()
    {
        // Unity特殊判空逻辑：同时检查引用和Unity内部销毁标记
        return currentPopup != null && !currentPopup.Equals(null);
    }
    // 新增安全获取方法
    // void Update()
    // {
    //     Debug.Log($"弹窗状态: {GetCurrentPopup() != null} | 激活状态: {IsPopupAlive()}");
    // }
    public GameObject GetCurrentPopup()
    {
        // Unity特殊判空逻辑
        if (currentPopup == null || currentPopup.Equals(null))
        {
            return null;
        }
        return currentPopup;
    }


    public void ClosePopup()
    {
        if (currentPopup == null) return;

        // 立即标记为null，防止其他脚本访问
        var popupToDestroy = currentPopup;
        currentPopup = null; 

        // 实际销毁操作
        Destroy(popupToDestroy);
        sensorWebsocket.SwitchState(false);
        // Debug.Log("弹窗销毁完成"); // DEBUG标记
    }

    // private IEnumerator DelayedDestroy()
    // {
    //     yield return new WaitForEndOfFrame();
    //
    //     if (currentPopup != null)
    //     {
    //         Destroy(currentPopup);
    //         currentPopup = null;
    //     }
    // }
}