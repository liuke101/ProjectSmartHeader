using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEditor;
using UnityEngine;

namespace INab.WorldScanFX.URP
{
    [CustomEditor(typeof(ScanFX))]
    public class ScanFXEditor : ScanFXBaseEditor
    {
        protected override void DrawPostProcess()
        {
            EditorGUILayout.LabelField("Post Process", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.HelpBox("Make sure to add ScanFXFeature to your URP renderer.", MessageType.Info);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(scanOrigin);
                EditorGUILayout.PropertyField(alwaysPassScanOriginPosition);
                EditorGUILayout.PropertyField(alwaysPassScanOriginDirection);
                if (scanOrigin.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Please assign a game object to the scanOrigin field", MessageType.Error);
                }
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(scanDuration);

                GUI.enabled = false;
                EditorGUILayout.PropertyField(scansLeft);
                EditorGUILayout.PropertyField(timeLeft);
                EditorGUILayout.PropertyField(timePassed);
                GUI.enabled = true;

                EditorGUILayout.Space();
            }
        }
    }
}