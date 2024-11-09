using INab.WorldScanFX;
using UnityEngine;

namespace Item.SmartHeader
{
    /// <summary>
    /// 智能头部
    /// </summary>
    [RequireComponent(typeof(SmartHeaderBuildTool))]
    public class SmartHeader : MonoBehaviour
    {
        //脚本界面添加一个按钮，点击执行SmartHeaderBuildTool的Init方法
        private SmartHeaderBuildTool smartHeaderBuildTool;
        
        private void Awake()
        {
            smartHeaderBuildTool = GetComponent<SmartHeaderBuildTool>();
        }
        
    }
}