using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 弹窗点击关闭组件，实现点击弹窗外部区域关闭功能
/// </summary>
public class PopupCloseOnClick : MonoBehaviour, IPointerClickHandler
{
    private RectTransform _popupContent; // 弹窗内容区域

    /// <summary>
    /// 初始化组件，获取弹窗内容区域引用
    /// </summary>
    void Start()
    {
        Transform contentTransform = transform.parent.Find("Content"); // 向上查找父级
        if (contentTransform == null)
        {
            Debug.LogError($"未找到Content对象！可用子对象: {GetChildNames()}");
            return;
        }

        _popupContent = contentTransform.GetComponent<RectTransform>();
        if (_popupContent == null)
        {
            Debug.LogError($"Content对象缺少RectTransform组件！对象层级: {GetFullPath(contentTransform)}");
        }
    }
    
    // 辅助方法：获取完整层级路径
    string GetFullPath(Transform obj)
    {
        List<string> path = new List<string>();
        while (obj != null)
        {
            path.Add(obj.name);
            obj = obj.parent;
        }
        path.Reverse();
        return string.Join("/", path);
    }
    // 辅助方法：获取子对象名称列表
    string GetChildNames()
    {
        return string.Join(", ", 
            GetComponentsInChildren<Transform>(true)
                .Select(t => t.name));
    }

    /// <summary>
    /// 处理指针点击事件
    /// </summary>
    /// <param name="eventData">指针事件数据，包含点击位置等信息</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (this == null || !isActiveAndEnabled || _popupContent == null) 
            return;

        // 添加点击位置可视化调试
        Debug.DrawRay(eventData.position, Vector3.forward * 10, Color.red, 2f);

        // 优化坐标转换逻辑
        Camera eventCamera = GetComponentInParent<Canvas>().worldCamera;
        bool isOverlay = GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _popupContent,
            eventData.position,
            isOverlay ? null : eventCamera,
            out Vector2 localPoint
        );

        // 添加容错阈值（2像素）
        Rect clickableArea = _popupContent.rect;

        if (!clickableArea.Contains(localPoint))
        {
            // Debug.Log($"触发关闭 | 点击坐标: {localPoint} | 内容区域: {_popupContent.rect}");
            SensorManager manager = FindObjectOfType<SensorManager>();
            if (manager == null)
            {
                Debug.LogError("SensorManager未找到！");
                return;
            }
            // Debug.Log($"弹窗实例: {manager}");
            // Debug.Log($"弹窗实例是否存在: {manager != null }");
            // Debug.Log($"弹窗实例是否存活: {manager.IsPopupAlive()}");
            // 通过公共方法获取弹窗实例
            GameObject currentPopup = manager.GetCurrentPopup();
            if (currentPopup == null)
            {
                Debug.LogError("未找到弹窗实例！");
                return;
            }
            // Debug.Log($"弹窗激活状态: {currentPopup.activeSelf} | 存活: {manager.IsPopupAlive()}");
            if (manager.IsPopupAlive())
            {
                // Debug.Log("正在关闭有效弹窗");
                manager.ClosePopup();
            }
            else
            {
                Debug.LogWarning("弹窗已销毁或未激活，无需关闭");
            }
        }
    }
}
