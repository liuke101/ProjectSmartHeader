using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerrainVisibilityController : MonoBehaviour
{
    public GameObject terrain2;
    public TextMeshProUGUI buttonText;  // 使用 TextMeshProUGUI 类型
    
    public void ToggleTerrain2Visibility()
    {
        bool isActive = terrain2.activeSelf;
        terrain2.SetActive(!isActive);  // 切换显隐状态

        // 更新按钮文本
        buttonText.text = isActive ? "显示山体" : "隐藏山体";
    }
}
