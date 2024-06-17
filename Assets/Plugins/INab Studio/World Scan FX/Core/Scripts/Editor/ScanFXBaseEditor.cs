using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace INab.WorldScanFX
{
    public class ScanFXBaseEditor : Editor
    {
        // Post Processing

        public SerializedProperty updateScanMaterialProperties;
        public SerializedProperty scanOrigin;
        public SerializedProperty alwaysPassScanOriginPosition;
        public SerializedProperty alwaysPassScanOriginDirection;

        public SerializedProperty scanDuration;
        public SerializedProperty scansLeft;
        public SerializedProperty timeLeft;
        public SerializedProperty timePassed;

        public SerializedProperty currentScanValueTesting;
        public SerializedProperty scansNumberTesting;

        public SerializedProperty scanMaterial;

        // PostProcessingMaterialProperties

        public SerializedProperty _SizeAdjust;
        public SerializedProperty _Size;
        public SerializedProperty _OriginOffset;
        
        public SerializedProperty _MaskRadius;
        public SerializedProperty _MaskHardness;
        public SerializedProperty _MaskPower;

        public SerializedProperty _FovMaskEnabled;
        public SerializedProperty _FovMask;
        public SerializedProperty _FovMaskSmoothness;

        public SerializedProperty _EdgeHardness;
        public SerializedProperty _EdgePower;
        public SerializedProperty _EdgeMultiplier;

        public SerializedProperty _HighlightPower;
        public SerializedProperty _HighlightMultiplier;

        public SerializedProperty _EdgeColor;
        public SerializedProperty _HighlightColor;

        public SerializedProperty _OverlayType;
        public SerializedProperty _OverlayMultiplier;
        public SerializedProperty _OverlayPower;
        public SerializedProperty _OverlayColor;
        public SerializedProperty _ScreenTexture;
        public SerializedProperty _ScreenTextureTiling;
        public SerializedProperty _Frequency;
        public SerializedProperty _Ratio;
        public SerializedProperty _Thickness;
        public SerializedProperty _NormalsOffset;
        public SerializedProperty _NormalsHardness;
        public SerializedProperty _NormalsPower;
        public SerializedProperty _DepthThreshold;
        public SerializedProperty _DepthHardness;
        public SerializedProperty _DepthPower;

        public SerializedProperty _EdgeDetectionMultiplier;
        public SerializedProperty _GridMultiplier;

        private bool waitForFavMaskEnabled = false;
        private bool waitForOverlayType = false;

        // CustomHighlight

        public SerializedProperty useCustomHighlight;
        public SerializedProperty DistanceOffset;
        public SerializedProperty MaskRadiusOffset;
        public SerializedProperty FovOffset;
        public SerializedProperty highlightObjects;
        public SerializedProperty useScanOverlayOnMaterials;
        public SerializedProperty highlightMaterials;
        public SerializedProperty useWorldScanMaterials;
        public SerializedProperty worldScanMaterials;


        public void OnEnable()
        {
            //currentCamera = serializedObject.FindProperty("currentCamera");
            updateScanMaterialProperties = serializedObject.FindProperty("updateScanMaterialProperties");
            scanMaterial = serializedObject.FindProperty("scanMaterial");
            scanOrigin = serializedObject.FindProperty("scanOrigin");
            alwaysPassScanOriginPosition = serializedObject.FindProperty("alwaysPassScanOriginPosition");
            alwaysPassScanOriginDirection = serializedObject.FindProperty("alwaysPassScanOriginDirection");

            scanDuration = serializedObject.FindProperty("scanDuration");
            scansLeft = serializedObject.FindProperty("scansLeft");
            timeLeft = serializedObject.FindProperty("timeLeft");
            timePassed = serializedObject.FindProperty("timePassed");

            currentScanValueTesting = serializedObject.FindProperty("currentScanValueTesting");
            scansNumberTesting = serializedObject.FindProperty("scansNumberTesting");

            _SizeAdjust = serializedObject.FindProperty("_SizeAdjust");
            _Size = serializedObject.FindProperty("_Size");
            _OriginOffset = serializedObject.FindProperty("_OriginOffset");
            _MaskRadius = serializedObject.FindProperty("_MaskRadius");
            _MaskHardness = serializedObject.FindProperty("_MaskHardness");
            _MaskPower = serializedObject.FindProperty("_MaskPower");
            _FovMaskEnabled = serializedObject.FindProperty("_FovMaskEnabled");
            _FovMask = serializedObject.FindProperty("_FovMask");


            _FovMaskSmoothness = serializedObject.FindProperty("_FovMaskSmoothness");

            _EdgeHardness = serializedObject.FindProperty("_EdgeHardness");
            _EdgePower = serializedObject.FindProperty("_EdgePower");
            _EdgeMultiplier = serializedObject.FindProperty("_EdgeMultiplier");
            _HighlightPower = serializedObject.FindProperty("_HighlightPower");
            _HighlightMultiplier = serializedObject.FindProperty("_HighlightMultiplier");

            _HighlightColor = serializedObject.FindProperty("_HighlightColor");
            _EdgeColor = serializedObject.FindProperty("_EdgeColor");

            _OverlayType = serializedObject.FindProperty("_OverlayType");
            _OverlayMultiplier = serializedObject.FindProperty("_OverlayMultiplier");
            _OverlayPower = serializedObject.FindProperty("_OverlayPower");
            _OverlayColor = serializedObject.FindProperty("_OverlayColor");
            _ScreenTexture = serializedObject.FindProperty("_ScreenTexture");
            _ScreenTextureTiling = serializedObject.FindProperty("_ScreenTextureTiling");
            _Frequency = serializedObject.FindProperty("_Frequency");
            _Ratio = serializedObject.FindProperty("_Ratio");
            _Thickness = serializedObject.FindProperty("_Thickness");
            _NormalsOffset = serializedObject.FindProperty("_NormalsOffset");
            _NormalsHardness = serializedObject.FindProperty("_NormalsHardness");

            _NormalsPower = serializedObject.FindProperty("_NormalsPower");
            _DepthThreshold = serializedObject.FindProperty("_DepthThreshold");
            _DepthHardness = serializedObject.FindProperty("_DepthHardness");
            _DepthPower = serializedObject.FindProperty("_DepthPower");

            _EdgeDetectionMultiplier = serializedObject.FindProperty("_EdgeDetectionMultiplier");
            _GridMultiplier = serializedObject.FindProperty("_GridMultiplier");

            useCustomHighlight = serializedObject.FindProperty("useCustomHighlight");
            DistanceOffset = serializedObject.FindProperty("DistanceOffset");
            MaskRadiusOffset = serializedObject.FindProperty("MaskRadiusOffset");
            FovOffset = serializedObject.FindProperty("FovOffset");
            highlightObjects = serializedObject.FindProperty("highlightObjects");
            useScanOverlayOnMaterials = serializedObject.FindProperty("useScanOverlayOnMaterials");
            highlightMaterials = serializedObject.FindProperty("highlightMaterials");
            useWorldScanMaterials = serializedObject.FindProperty("useWorldScanMaterials");
            worldScanMaterials = serializedObject.FindProperty("worldScanMaterials");
        }

        protected virtual void DrawPostProcess()
        {
            EditorGUILayout.LabelField("Post Process", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
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

        protected void DrawCustomHighlight()
        {
            EditorGUILayout.LabelField("Other", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(useCustomHighlight);
                if (useCustomHighlight.boolValue)
                {
                    EditorGUILayout.PropertyField(DistanceOffset);
                    EditorGUILayout.PropertyField(MaskRadiusOffset);
                    EditorGUILayout.PropertyField(FovOffset);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(highlightObjects);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(useScanOverlayOnMaterials);
                if (useScanOverlayOnMaterials.boolValue)
                {
                    EditorGUILayout.HelpBox("Make sure to turn on UseScanOvelay keyword in the Scan FX Highlight materials", MessageType.Info);
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(highlightMaterials);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(useWorldScanMaterials);
                if (useWorldScanMaterials.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(worldScanMaterials);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Space();
            }
        }

        protected void DrawEditorTesting(ScanFXBase scanFX)
        {
            EditorGUILayout.LabelField("Testing", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.HelpBox("Use the currentScanValueTesting field to check the effect's appearance. It ranges from 0 to 1, after which it loops back to the beginning.", MessageType.Info);
                EditorGUILayout.PropertyField(currentScanValueTesting);

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("In play mode, activate StartScans to initiate a sequence of scans. The total number of scans will match the value set in scansNumberTesting.", MessageType.Info);
                if (GUILayout.Button("StartScans"))
                {
                    scanFX.PassScanOriginProperties();
                    scanFX.StartScan(scanFX.scansNumberTesting);
                }
                EditorGUILayout.PropertyField(scansNumberTesting);
                EditorGUILayout.Space();
            }
        }

        protected void DrawScanMaterialSettings()
        {
            EditorGUILayout.LabelField("Scan Material", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(scanMaterial);
                if (scanMaterial.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox("Please assign a Scan FX material to the scanMaterial field", MessageType.Error);
                }
                EditorGUILayout.PropertyField(updateScanMaterialProperties);
                EditorGUILayout.Space();

                if (updateScanMaterialProperties.boolValue == false)
                {
                    return;
                }
                EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_Size);
                EditorGUILayout.PropertyField(_SizeAdjust);
                EditorGUILayout.PropertyField(_OriginOffset);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_EdgeColor);
                EditorGUILayout.PropertyField(_EdgeHardness);
                EditorGUILayout.PropertyField(_EdgePower);
                EditorGUILayout.PropertyField(_EdgeMultiplier);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(_HighlightColor);
                EditorGUILayout.PropertyField(_HighlightPower);
                EditorGUILayout.PropertyField(_HighlightMultiplier);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Sphere Mask", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_MaskRadius);
                EditorGUILayout.PropertyField(_MaskHardness);
                EditorGUILayout.PropertyField(_MaskPower);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Fov Mask", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_FovMaskEnabled);
                if (_FovMaskEnabled.boolValue)
                {
                    EditorGUILayout.PropertyField(_FovMask);
                    EditorGUILayout.PropertyField(_FovMaskSmoothness);
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Overlay", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_OverlayType);

                if (_OverlayType.enumValueIndex != 0)
                {
                    EditorGUILayout.PropertyField(_OverlayMultiplier);
                    EditorGUILayout.PropertyField(_OverlayPower);
                    EditorGUILayout.PropertyField(_OverlayColor);
                    EditorGUILayout.Space();
                }

                switch (_OverlayType.enumValueIndex)
                {
                    case 1:
                        EditorGUILayout.PropertyField(_ScreenTexture);
                        EditorGUILayout.PropertyField(_ScreenTextureTiling);
                        break;
                    case 2:
                        EditorGUILayout.PropertyField(_Thickness);

                        EditorGUILayout.PropertyField(_NormalsOffset);
                        EditorGUILayout.PropertyField(_NormalsHardness);
                        EditorGUILayout.PropertyField(_NormalsPower);

                        EditorGUILayout.PropertyField(_DepthThreshold);
                        EditorGUILayout.PropertyField(_DepthHardness);
                        EditorGUILayout.PropertyField(_DepthPower);
                        break;
                    case 3:
                        EditorGUILayout.PropertyField(_Frequency);
                        EditorGUILayout.PropertyField(_Ratio);
                        break;
                    case 4:

                        EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(_GridMultiplier);
                        EditorGUILayout.PropertyField(_Frequency);
                        EditorGUILayout.PropertyField(_Ratio);
                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Edge Detection", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(_EdgeDetectionMultiplier);
                        EditorGUILayout.PropertyField(_Thickness);

                        EditorGUILayout.PropertyField(_NormalsOffset);
                        EditorGUILayout.PropertyField(_NormalsHardness);
                        EditorGUILayout.PropertyField(_NormalsPower);

                        EditorGUILayout.PropertyField(_DepthThreshold);
                        EditorGUILayout.PropertyField(_DepthHardness);
                        EditorGUILayout.PropertyField(_DepthPower);
                        break;
                }

                EditorGUILayout.Space();
            }
        }

        protected void HaveKeywordsChanged(ScanFXBase scanFX)
        {
            if ((scanFX._FovMaskEnabled) != _FovMaskEnabled.boolValue)
            {
                waitForFavMaskEnabled = true;
            }

            if ((int)(scanFX._OverlayType) != _OverlayType.enumValueIndex)
            {
                waitForOverlayType = true;
            }
        }

        protected virtual void UpdateKeywords(ScanFXBase scanFX)
        {
            if (waitForFavMaskEnabled || waitForOverlayType)
            {
                scanFX.UpdateAllMaterialsKeywords();
                waitForFavMaskEnabled = false;
                waitForOverlayType = false;
            }
        }

        public override void OnInspectorGUI()
        {
            //DrawDefaultInspector();
            serializedObject.Update();

            // get target
            ScanFXBase scanFX = (ScanFXBase)target;

            DrawPostProcess();
            DrawEditorTesting(scanFX);
            DrawScanMaterialSettings();
            DrawCustomHighlight();

            HaveKeywordsChanged(scanFX);

            serializedObject.ApplyModifiedProperties();

            UpdateKeywords(scanFX);
        }
    }
}