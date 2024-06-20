using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace INab.WorldScanFX
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScanFXHighlight))]
    public class ScanFXHighlightEditor : Editor
    {
        // all serialized properties
        SerializedProperty renderers;
        SerializedProperty highlightDuration;
        SerializedProperty curve;
        SerializedProperty highlightEvent;

        public void OnEnable()
        {
            renderers = serializedObject.FindProperty("renderers");
            highlightDuration = serializedObject.FindProperty("highlightDuration");
            curve = serializedObject.FindProperty("curve");
            highlightEvent = serializedObject.FindProperty("highlightEvent");
        }

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            serializedObject.Update();

            ScanFXHighlight scanFXHighlight = (ScanFXHighlight)target;

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(renderers);
                EditorGUI.indentLevel--;
                if (GUILayout.Button("Find Renderers"))
                {
                    scanFXHighlight.FindRenderers();
                }

                if (GUILayout.Button("Find Renderers in children"))
                {
                    scanFXHighlight.FindRenderersInChildren();
                }
                EditorGUILayout.Space();
            }

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(highlightDuration);
                EditorGUILayout.PropertyField(curve);
                EditorGUILayout.PropertyField(highlightEvent);

            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}