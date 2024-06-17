using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace INab.WorldScanFX
{
    public abstract class ScanFXBase : MonoBehaviour
    {
        #region Static

        // Defines the type of overlay effect applied during the scanning process.
        public enum OverlayType
        {
            None, // No overlay.
            ScreenTexture, // Applies a texture overlay.
            EdgeDetection, // Highlights edges.
            Grid, // Displays a grid pattern.
            GridAndEdgeDetection // Combines grid and edge detection overlays.
        }

        // Keywords for enabling specific overlay shaders in materials.
        private static List<string> overlayTypeKeywords = new List<string>()
        {
            "_OVERLAY_NONE",
            "_OVERLAY_SCREENTEXTURE",
            "_OVERLAY_EDGEDETECTION",
            "_OVERLAY_GRID",
            "_OVERLAY_GRIDANDEDGEDETECTION"

        };

        // Keyword for enabling the field of view mask in materials.
        private static string fovMaskEnabledKeyword = "_FOVMASKENABLED";

        #endregion

        #region LogicProperties

        // General settings

        [Tooltip("Enables updating the scan material properties based on this script's settings.")]
        [SerializeField] private bool updateScanMaterialProperties = false;

        [Tooltip("Material used for the scan effect.")]
        [SerializeField] protected Material scanMaterial;

        [Tooltip("Origin point for the scan effect.")]
        [SerializeField] public Transform scanOrigin;

        [Tooltip("Disable to manually update the scan origin's position from code.")]
        [SerializeField] public bool alwaysPassScanOriginPosition = true;

        [Tooltip("Disable to manually update the scan origin's direction from code.")]
        [SerializeField] public bool alwaysPassScanOriginDirection = true;

        // Internal use for tracking origin position and direction
        private Vector3 originForward;
        private Vector3 originPosition;

        // Scan logic

        [Tooltip("How long each scan lasts, in seconds.")]
        [SerializeField] public float scanDuration = 5f;

        [Tooltip("Tracks the number of scans left to be executed.")]
        public int ScansLeft
        {
            get { return scansLeft; }
            private set { }
        }

        [Tooltip("Counts how many scans are remaining.")]
        [SerializeField] private int scansLeft = 0;

        [Tooltip("Time remaining for the current scan to finish.")]
        [SerializeField] private float timeLeft = 0f;

        [Tooltip("Time elapsed since the current scan started.")]
        [SerializeField] private float timePassed = 0f;

        // Editor Testing

        [Tooltip("Adjusts the scan value for editor testing purposes.")]
        [Range(0f, 2f)]
        [SerializeField] private float currentScanValueTesting = 1f;

        [Tooltip("Sets the number of scans for testing.")]
        [SerializeField] public int scansNumberTesting = 3;

        #endregion

        #region PostProcessingMaterialProperties

        // Appearance
        [Tooltip("Base size of the scan effect.")]
        [SerializeField] private float _Size = 8f;

        [Tooltip("Adjust this to ensure a smooth fade-out at the end of the scan. Test with the current scan value.")]
        [SerializeField] private float _SizeAdjust = 0f;

        [Tooltip("Offset from the origin to exclude nearby objects from the scan effect, like the player.")]
        [SerializeField] private float _OriginOffset = 2f;

        [Tooltip("Color of the scan's edge.")]
        [SerializeField][ColorUsage(true, true)] private Color _EdgeColor = new Color(0.09803922f, 0.2039216f, 1f);

        [Tooltip("Sharpness of the scan's edge.")]
        [SerializeField][Range(0f, 1f)] private float _EdgeHardness = 0.8f;

        [Tooltip("Adjusts the edge's size and appearance.")]
        [SerializeField] private float _EdgePower = 10f;

        [Tooltip("Multiplier for the edge effect.")]
        [SerializeField] private float _EdgeMultiplier = 1f;

        [Tooltip("Color for the slight highlight of the scan.")]
        [SerializeField][ColorUsage(true, true)] private Color _HighlightColor = new Color(0.454902f, 0.4980392f, 1f);

        [Tooltip("Adjusts the highlight's size and appearance.")]
        [SerializeField] private float _HighlightPower = 3f;

        [Tooltip("Multiplier for the highlight effect.")]
        [SerializeField][Range(0f, 1f)] private float _HighlightMultiplier = 0.8f;

        // Sphere Mask
        [Tooltip("Radius of the scan area.")]
        [SerializeField] private float _MaskRadius = 50f;

        [Tooltip("Defines the edge sharpness of the sphere mask.")]
        [SerializeField][Range(0f, 1f)] private float _MaskHardness = 0.5f;

        [Tooltip("Adjusts the scan radius mask's intensity.")]
        [SerializeField] private float _MaskPower = 1f;

        // Fov Mask
        [Tooltip("Toggle the FOV mask on or off.")]
        [SerializeField] public bool _FovMaskEnabled = false;

        [Tooltip("FOV mask value, simulating an angle from 0 to 180 degrees.")]
        [SerializeField][Range(0f, 1f)] private float _FovMask = 0.5f;

        [Tooltip("Smoothness of the FOV mask edges.")]
        [SerializeField][Range(0f, 1f)] private float _FovMaskSmoothness = 0.2f;

        // Overlay
        [Tooltip("Type of overlay effect.")]
        [SerializeField] public OverlayType _OverlayType = OverlayType.None;

        [Tooltip("Multiplier for the overlay effect.")]
        [SerializeField][Range(0f, 1f)] private float _OverlayMultiplier = 1f;

        [Tooltip("Intensity of the overlay effect.")]
        [SerializeField] private float _OverlayPower = 1f;

        [Tooltip("Color of the overlay effect.")]
        [SerializeField][ColorUsage(true, true)] private Color _OverlayColor = Color.white;

        // Screen Texture
        [Tooltip("Texture used for the screen overlay.")]
        [SerializeField] private Texture2D _ScreenTexture = null;

        [Tooltip("Tiling rate of the screen texture.")]
        [SerializeField] private float _ScreenTextureTiling = 50f;

        // Grid
        [Tooltip("Frequency of the grid pattern.")]
        [SerializeField][Range(0f, 3f)] private float _Frequency = 1f;

        [Tooltip("Thickness of the grid lines.")]
        [SerializeField][Range(0.5f, 1)] private float _Ratio = 0.50f;

        // Edge Detection
        [Tooltip("Thickness of the detected edges.")]
        [SerializeField][Range(0, 5)] private float _Thickness = 1.0f;

        [Tooltip("Offset for normals in edge detection.")]
        [SerializeField][Range(.01f, 1.5f)] private float _NormalsOffset = 0.1f;

        [Tooltip("Hardness of the normals edge detection.")]
        [SerializeField][Range(0, .99f)] private float _NormalsHardness = 0;

        [Tooltip("Enhances the effect of normals in edge detection.")]
        [SerializeField][Range(1, 5)] private float _NormalsPower = 1;

        [Tooltip("Threshold for depth in edge detection.")]
        [SerializeField][Range(0, 3)] private float _DepthThreshold = 1;

        [Tooltip("Hardness of the depth edge detection.")]
        [SerializeField][Range(0, 1)] private float _DepthHardness = .9f;

        [Tooltip("Enhances the effect of depth in edge detection.")]
        [SerializeField][Range(1, 5)] private float _DepthPower = 5;

        // Grid + Edge Detection
        [Tooltip("Enhances the edge detection effect within the grid.")]
        [SerializeField][Range(0f, 1f)] private float _EdgeDetectionMultiplier = 1f;

        [Tooltip("Enhances the grid effect.")]
        [SerializeField][Range(0f, 1f)] private float _GridMultiplier = 1f;

        #endregion

        #region FeaturesProperties

        // Used for detecting and highlighting objects in the scan range

        [Tooltip("Enables custom highlighting of objects within the scan range.")]
        [SerializeField] private bool useCustomHighlight = false;

        [Tooltip("Fine-tunes the point of triggering the highlight.")]
        [SerializeField] private float DistanceOffset = 0f;

        [Tooltip("Adjusts the maximum radius within which highlighting can be triggered.")]
        [SerializeField] private float MaskRadiusOffset = 0f;

        [Tooltip("Controls the adjustment of FOV for triggering the highlighters.")]
        [Range(-0.1f, 0.3f)]
        [SerializeField] private float FovOffset = 0f;

        [Tooltip("List of objects that will be highlighted during the scan.")]
        [SerializeField] public List<ScanFXHighlight> highlightObjects = new List<ScanFXHighlight>();

        // Used for having actual scan FX mask overlay on the highlighted objects

        [Tooltip("When enabled, highlighting on objects will be visible only within the actual scan range. Useful when using FOV mask.")]
        [SerializeField] private bool useScanOverlayOnMaterials = false;

        [Tooltip("List of materials to which the scan FX mask overlay will be applied.")]
        [SerializeField] public List<Material> highlightMaterials = new List<Material>();

        // ScanFX for world-spaced shaders instead of post-processing effect. Useful for transparent objects, for example.

        [Tooltip("Enables the use of world scan materials, applied to objects for a world-space shader effect rather than a post-processing effect. Useful for transparent objects.")]
        [SerializeField] private bool useWorldScanMaterials = false;

        [Tooltip("List of world scan materials to be used for applying the scan effect in world space.")]
        [SerializeField] public List<Material> worldScanMaterials = new List<Material>();


        #endregion

        #region PublicMethods

        /// <summary>
        /// Initiates the scanning effect with a specified number of scans.
        /// </summary>
        /// <param name="ScansNumber">The number of scans to enqueue.</param>
        public void StartScan(int ScansNumber)
        {
            if (scansLeft == 0)
            {
                scansLeft = ScansNumber;
                timeLeft = scanDuration;
            }
            else
            {
                scansLeft += ScansNumber;
            }

            // All logic is in the update method
        }

        /// <summary>
        /// Updates the position of the scan origin in the scan materials.
        /// </summary>
        public void PassScanOriginPosition()
        {
            PassCustomScanOriginPosition(scanOrigin);
        }

        /// <summary>
        /// Updates the forward direction of the scan origin in the scan materials.
        /// </summary>
        public void PassScanOriginDirection()
        {
            PassCustomScanOriginDirection(scanOrigin);
        }

        /// <summary>
        /// Updates both the position and direction of the scan origin in the scan materials.
        /// </summary>
        public void PassScanOriginProperties()
        {
            PassCustomScanOriginProperties(scanOrigin);
        }

        /// <summary>
        /// Passes the specified transform's position and direction properties to the scan materials.
        /// </summary>
        /// <param name="customScanOrigin">The transform that should be used as the scan origin.</param>

        public void PassCustomScanOriginProperties(Transform customScanOrigin)
        {
            PassCustomScanOriginPosition(customScanOrigin);
            PassCustomScanOriginDirection(customScanOrigin);
        }

        /// <summary>
        /// Passes the specified transform's position property to the scan materials.
        /// </summary>
        /// <param name="customScanOrigin">The transform that should be used as the scan origin.</param>
        public void PassCustomScanOriginPosition(Transform customScanOrigin)
        {
            originPosition = customScanOrigin.position;

            scanMaterial.SetVector("_Origin", originPosition);

            if (useScanOverlayOnMaterials)
            {
                foreach (var material in highlightMaterials)
                {
                    material.SetVector("_Origin", originPosition);
                }
            }

            if (useWorldScanMaterials)
            {
                foreach (var material in worldScanMaterials)
                {
                    material.SetVector("_Origin", originPosition);
                }
            }
        }

        /// <summary>
        /// Passes the specified transform's forward direction property to the scan materials.
        /// </summary>
        /// <param name="customScanOrigin">The transform that should be used as the scan origin.</param>
        public void PassCustomScanOriginDirection(Transform customScanOrigin)
        {
            originForward = customScanOrigin.forward;

            scanMaterial.SetVector("_Forward", originForward);

            if (useScanOverlayOnMaterials)
            {
                foreach (var material in highlightMaterials)
                {
                    material.SetVector("_Forward", originForward);
                }
            }

            if (useWorldScanMaterials)
            {
                foreach (var material in worldScanMaterials)
                {
                    material.SetVector("_Forward", originForward);
                }
            }
        }

        #endregion

        #region UnityMethods

        /// <summary>
        /// Sets up initial scan values and updates material keywords.
        /// </summary>
        public void Start()
        {
            SetCurrentScanValue(0);
            if(updateScanMaterialProperties || useScanOverlayOnMaterials || useWorldScanMaterials) UpdateAllMaterialsKeywords();

            foreach (var item in highlightObjects)
            {
                item.AlreadyScanned = false;
            }
        }

        /// <summary>
        /// Handles the scanning logic and updates per frame.
        /// </summary>
        public void Update()
        {
            if (scanOrigin == null || scanMaterial == null)
            {
                return;
            }

            // Bug fix, See: CustomHighlightObjects
            if (scanOrigin.position == Vector3.zero)
            {
                scanOrigin.position = scanOrigin.position + Vector3.one * 0.001f;
            }

            if (alwaysPassScanOriginPosition) PassScanOriginPosition();
            if (alwaysPassScanOriginDirection) PassScanOriginDirection();

            // Editor Testing
            if (!Application.isPlaying)
            {
                SetCurrentScanValue(currentScanValueTesting);

                if (!alwaysPassScanOriginPosition) PassScanOriginPosition();
                if (!alwaysPassScanOriginDirection) PassScanOriginDirection();
            }

            // Scan logic 
            if (scansLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                timePassed += Time.deltaTime;

                float currentScanValue = timePassed / scanDuration;

                SetCurrentScanValue(currentScanValue);

                if (timeLeft > 0) CustomHighlightObjects(currentScanValue);

                if (timeLeft <= 0)
                {
                    scansLeft--;

                    foreach (var item in highlightObjects)
                    {
                        item.AlreadyScanned = false;
                    }

                    if (scansLeft > 0)
                    {
                        timeLeft = scanDuration;
                    }
                    else
                    {
                        timePassed = 0;
                        timeLeft = 9999;

                        // 1 in order to make highlightMaterials scan mask work
                        SetCurrentScanValue(1);
                    }
                }
            }
        }

        private void OnValidate()
        {
            if (updateScanMaterialProperties)
            {
                if (scanMaterial != null)
                {
                    SetCommonScanProperties(scanMaterial);
                    SetPostProcessScanProperties(scanMaterial);
                }
            }

            if (useScanOverlayOnMaterials)
            {
                foreach (var material in highlightMaterials)
                {
                    SetCommonScanProperties(material);
                }
            }

            if (useWorldScanMaterials)
            {
                foreach (var material in worldScanMaterials)
                {
                    SetCommonScanProperties(material);
                    SetPostProcessScanProperties(material);
                }
            }
        }

        #endregion

        #region PrivateMethods

        private void SetCurrentScanValue(float value)
        {
            scanMaterial.SetFloat("_CurrentScanValue", value);

            if (useScanOverlayOnMaterials)
            {
                foreach (var material in highlightMaterials)
                {
                    material.SetFloat("_CurrentScanValue", value);
                }
            }

            if (useWorldScanMaterials)
            {
                foreach (var material in worldScanMaterials)
                {
                    material.SetFloat("_CurrentScanValue", value);
                }
            }
        }

        private void CustomHighlightObjects(float currentScanValue)
        {
            if (useCustomHighlight)
            {
                // Go through all the game objects positions and calculate when we should enable the highlight
                foreach (var item in highlightObjects)
                {
                    if (item == null) continue;
                    if (item.gameObject.activeInHierarchy == false || item.enabled == false) continue;

                    // If we already scanned the object in current scan, skip it
                    if (item.AlreadyScanned) continue;

                    // Bad fix for the bug when Always Pass Scan Origin is off we get 0,0,0 position AND the good position
                    if (originPosition == Vector3.zero) continue;

                    Vector3 worldPosition = item.transform.position;

                    float distance = Vector3.Distance(worldPosition, originPosition) + DistanceOffset;

                    float fmod = _SizeAdjust + 1;
                    float scan = ((currentScanValue * fmod) % fmod) * (_MaskRadius);

                    float output = distance - scan > 1 ? 0 : 1;

                    // Make sure that no object outside the mask radius is highlighted
                    float maskOffset = (_MaskRadius - MaskRadiusOffset) > distance ? 1 : 0;
                    output *= maskOffset;

                    if (_FovMaskEnabled)
                    {
                        Vector2 wPos = new Vector2(worldPosition.x, worldPosition.z);
                        Vector2 oPos = new Vector2(originPosition.x, originPosition.z);

                        Vector2 dot2 = (wPos - oPos).normalized;
                        Vector2 forward = new Vector2(originForward.x, originForward.z);

                        float dot = Vector2.Dot(dot2, forward);
                        dot = Mathf.Clamp(dot, 0, 1);

                        float fovMask = dot > 1 - (_FovMask + FovOffset) ? 1 : 0;

                        output *= fovMask;
                    }


                    bool inScanRange = true;
                    if (output == 0) inScanRange = false;

                    if (inScanRange)
                    {
                        item.PlayEffect();
                    }
                }
            }
        }

        #endregion

        #region MaterialsMethods

        /// <summary>
        /// Updates keywords on materials based on current overlay and FOV mask settings.
        /// </summary>
        public void UpdateAllMaterialsKeywords()
        {
            UpdateMaterialKeyowrds(scanMaterial);

            if (useScanOverlayOnMaterials)
            {
                foreach (var material in highlightMaterials)
                {
                    UpdateMaterialKeyowrds(material);
                }
            }

            if (useWorldScanMaterials)
            {
                foreach (var material in worldScanMaterials)
                {
                    UpdateMaterialKeyowrds(material);
                }
            }
        }

        private void UpdateMaterialKeyowrds(Material material)
        {
            foreach (var keyword in material.enabledKeywords)
            {
                if (overlayTypeKeywords.Contains(keyword.name))
                {
                    material.DisableKeyword(keyword);
                }

                if (keyword.name == fovMaskEnabledKeyword)
                {
                    material.DisableKeyword(fovMaskEnabledKeyword);
                }
            }

            material.EnableKeyword(overlayTypeKeywords[(int)_OverlayType]);
            if (_FovMaskEnabled)
            {
                material.EnableKeyword(fovMaskEnabledKeyword);
            }
        }

        private void SetCommonScanProperties(Material material)
        {
            material.SetFloat("_SizeAdjust", _SizeAdjust);
            material.SetFloat("_Size", _Size);
            material.SetFloat("_OriginOffset", _OriginOffset);

            material.SetFloat("_MaskRadius", _MaskRadius);
            material.SetFloat("_MaskHardness", _MaskHardness);
            material.SetFloat("_MaskPower", _MaskPower);

            material.SetFloat("_FovMask", _FovMask);
            material.SetFloat("_FovMaskSmoothness", _FovMaskSmoothness);

            material.SetFloat("_EdgeHardness", _EdgeHardness);
        }

        private void SetPostProcessScanProperties(Material material)
        {
            material.SetColor("_EdgeColor", _EdgeColor);
            material.SetColor("_HighlightColor", _HighlightColor);

            material.SetFloat("_EdgePower", _EdgePower);
            material.SetFloat("_EdgeMultiplier", _EdgeMultiplier);

            material.SetFloat("_HighlightPower", _HighlightPower);
            material.SetFloat("_HighlightMultiplier", _HighlightMultiplier);

            material.SetFloat("_OverlayMultiplier", _OverlayMultiplier);
            material.SetFloat("_OverlayPower", _OverlayPower);
            material.SetColor("_OverlayColor", _OverlayColor);

            material.SetFloat("_Frequency", _Frequency);
            material.SetFloat("_Ratio", _Ratio);

            material.SetFloat("_ScreenTextureTiling", _ScreenTextureTiling);
            material.SetTexture("_ScreenTexture", _ScreenTexture != null ? _ScreenTexture : Texture2D.whiteTexture);

            material.SetFloat("_Thickness", _Thickness);
            material.SetFloat("_NormalsOffset", _NormalsOffset);
            material.SetFloat("_NormalsHardness", _NormalsHardness);
            material.SetFloat("_NormalsPower", _NormalsPower);
            material.SetFloat("_DepthThreshold", _DepthThreshold);
            material.SetFloat("_DepthHardness", _DepthHardness);
            material.SetFloat("_DepthPower", _DepthPower);

            material.SetFloat("_EdgeDetectionMultiplier", _EdgeDetectionMultiplier);
            material.SetFloat("_GridMultiplier", _GridMultiplier);
        }
        #endregion
    }
}