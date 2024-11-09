// SmartHeaderEditor.cs

using Item.SmartHeader;
using UnityEngine;
using UnityEditor;

/// <summary>
/// SmartHeaderBuildTool编辑器拓展
/// </summary>
[CustomEditor(typeof(SmartHeaderBuildTool))]
public class SmartHeaderToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //调用SmartHeaderBuildTool的Init方法
        if (GUILayout.Button("初始化智能头部模型"))
        {
            SmartHeaderBuildTool smartHeaderBuildTool = (SmartHeaderBuildTool)target;
            smartHeaderBuildTool.Init();
        }
    }
}