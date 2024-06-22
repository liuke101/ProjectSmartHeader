using UnityEditor;
using UnityEngine;

namespace JustAssets.TerrainUtility
{
    [CustomEditor(typeof(ToggleGameObjects))]
    public class ToggleGameObjectsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var t = (ToggleGameObjects)target;
            if (GUILayout.Button("Toggle"))
            {
                t.Toggle();
            }

            DrawDefaultInspector();
        }
    }
}