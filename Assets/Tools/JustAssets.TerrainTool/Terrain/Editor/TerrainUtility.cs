using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JustAssets.TerrainUtility.Rating;
using UnityEditor;
using UnityEngine;

namespace JustAssets.TerrainUtility
{
    public class TerrainUtility : EditorWindow
    {
        public const string PRODUCT_NAME = "Terrain Utility";
        public const string SUPPORT_LINK = "https://justassets.atlassian.net/servicedesk";

        public enum Operation
        {
            None,

            SplitTerrain,

            MergeTerrain
        }

        private bool _execute;

        private TerrainUtilityUIData _iconData;

        private Operation _operation = Operation.None;

        private readonly List<Operation> _operations =
            Enum.GetValues(typeof(Operation)).OfType<Operation>().Skip(1).ToList();

        private Dictionary<Operation, Option> _options;
        private Styles _styles;

        private Terrain[] _terrainSelection;

        private readonly Version _version = new Version(2, 1, 2);

        private Vector2 _view;

        [MenuItem("Tools/Terrain/Terrain Utility")]
        public static void ShowWindow()
        {
            var window = GetWindow<TerrainUtility>(PRODUCT_NAME);
            window.titleContent = Styles.TitleContent;
            window.UpdateSelection();
        }
        
        public void OnEnable()
        {
            var assetGUID = AssetDatabase.FindAssets($"t:{nameof(TerrainUtilityUIData)} {nameof(TerrainUtilityUIData)}")
                .FirstOrDefault();
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            _iconData = AssetDatabase.LoadAssetAtPath<TerrainUtilityUIData>(assetPath);

            _options = new Dictionary<Operation, Option>
            {
                {
                    Operation.SplitTerrain,
                    new Option(Operation.SplitTerrain,
                        new GUIContent("Split terrain(s)", _iconData.SplitTerrain,
                            "Splits all selected terrains into n pieces."), new SplitTerrain())
                },
                {
                    Operation.MergeTerrain,
                    new Option(Operation.MergeTerrain,
                        new GUIContent("Merge terrains", _iconData.MergeTerrain, "Merges selected terrains together."),
                        new MergeTerrain())
                }
            };
        }

        public void OnSelectionChange()
        {
            UpdateSelection();
        }

        private void OnGUI()
        {
            if (_styles == null)
                InitStyles();

            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

            HeaderBar();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(Styles.IconTerrain), GUILayout.Width(48),
                GUILayout.Height(48));
            EditorGUILayout.LabelField($"Terrain Utility {_version.ToString(3)}", _styles.Header,
                GUILayout.Height(48), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            {
                var iconSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(new Vector2(48, 48));
                EditorGUILayout.BeginVertical();
                for (var index = 0; index < _operations.Count; index++)
                {
                    if (index % 3 == 0) EditorGUILayout.BeginHorizontal();

                    var operation = _operations[index];
                    var style = new GUIStyle(EditorStyles.miniButton)
                        {stretchHeight = true, fixedHeight = 0, imagePosition = ImagePosition.ImageAbove};

                    var height = 80;
                    if (_options.TryGetValue(operation, out var option) && GUILayout.Button(option.Icon, style,
                            GUILayout.Width(height * 120 / 100), GUILayout.Height(height))) _operation = operation;

                    if (index % 3 == 2 || index == _operations.Count - 1) EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
                EditorGUIUtility.SetIconSize(iconSize);
            }
            EditorGUILayout.EndHorizontal();

            _options.TryGetValue(_operation, out var activeOption);
            if (activeOption != null)
            {
                EditorGUILayout.LabelField(activeOption.Icon.text, EditorStyles.boldLabel);

                EditorGUILayout.HelpBox(activeOption.Icon.tooltip, MessageType.Info);

                var isValid = activeOption.Handler.IsValid(out var reason);
                if (reason != null)
                    EditorGUILayout.HelpBox(reason, isValid ? MessageType.Warning : MessageType.Error);

                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                switch (_operation)
                {
                    case Operation.SplitTerrain:
                        var handler = _options[Operation.SplitTerrain].Handler;
                        var currentTilesPerAxis = (int)handler[1];
                        var tilesPerAxis = EditorGUILayout.IntSlider("Tiling", currentTilesPerAxis, 2, 64);

                        handler[1] = tilesPerAxis;
                        EditorGUILayout.LabelField(" ", $"{tilesPerAxis} x {tilesPerAxis} = {tilesPerAxis*tilesPerAxis} tiles", EditorStyles.boldLabel);
                        handler[0] = EditorGUILayout.ToggleLeft("Split Trees", (bool)handler[0]);
                        handler[2] = EditorGUILayout.ToggleLeft("Deactivate Source", (bool)handler[2]);

                        break;
                    case Operation.MergeTerrain:
                        MergeTerrain handlerMerge = (MergeTerrain)_options[Operation.MergeTerrain].Handler;
                        handlerMerge[0] = EditorGUILayout.ToggleLeft("Deactivate Sources", (bool) handlerMerge[0]);
                        var ignoreLayerOffsetNew = EditorGUILayout.ToggleLeft("Ignore Layer Offset", (bool)handlerMerge[1]);

                        if ((bool)handlerMerge[1] != ignoreLayerOffsetNew)
                            UpdateSelection();

                        handlerMerge[1] = ignoreLayerOffsetNew;

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Expected layers: ", handlerMerge.LayerCountToExpect.ToString());

                        break;
                }

                _view = EditorGUILayout.BeginScrollView(_view);
                {
                    EditorGUILayout.LabelField("Selected terrains", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Count: ", _terrainSelection.Length.ToString(), EditorStyles.miniLabel);
                    EditorGUIUtility.labelWidth = 0;
                    foreach (var terrain in _terrainSelection)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(terrain, typeof(Terrain), false);
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                {
                    GUI.enabled = isValid;
                    if (activeOption.Handler != null)
                        if (GUILayout.Button(activeOption.Icon.text + " now"))
                            _execute = true;

                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a tool to show details.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void Update()
        {
            if (!_execute)
                return;

            _execute = false;

            if (!_options.TryGetValue(_operation, out var option))
                return;

            option.Handler.Execute();

            _operation = Operation.None;
        }

        private void UpdateSelection()
        {
            _terrainSelection = Selection.gameObjects.SelectMany(x => x.GetComponentsInChildren<Terrain>()).ToArray();

            foreach (var option in _options.Values)
            {
                option.Handler.Selection = _terrainSelection;
            }

            Repaint();
        }

        private void InitStyles()
        {
            _styles = new Styles();
        }

        private static void HeaderBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                EditorGUILayout.LabelField("", GUILayout.ExpandWidth(true));
               
                if (GUILayout.Button("Rate", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    ReviewDialogWindow.ShowWindow();
                }

                if (GUILayout.Button("Support", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    if (EditorUtility.DisplayDialog("Support",
                            "If you found a bug or have a feature request please get in touch with us through the support webpage.", "Open webpage", "Cancel"))
                        Process.Start(SUPPORT_LINK);
                }

            }
            EditorGUILayout.EndHorizontal();
        }
        
        internal class Styles
        {
            public static readonly Texture2D IconTerrain = AssetPreview.GetMiniTypeThumbnail(typeof(Terrain));
            
            public static GUIContent TitleContent = new GUIContent("Terrain Utility",
                ScaledTexture2D(IconTerrain, 16, 16), "Tool window to split and merge terrains");

            public readonly GUIContent GuiTerrainIcon = new GUIContent(IconTerrain);

            public readonly GUIStyle Header;

            public Styles()
            {
                var largeLabel = new GUIStyle(EditorStyles.largeLabel);
                largeLabel.fontSize = 18;
                largeLabel.alignment = TextAnchor.MiddleLeft;
                Header = largeLabel;
            }

            private static Texture2D ScaledTexture2D(Texture2D original, int targetWidth, int targetHeight)
            {
                var rt = new RenderTexture(targetWidth, targetHeight, 24);
                RenderTexture.active = rt;
                Graphics.Blit(original, rt);
                var result = new Texture2D(targetWidth, targetHeight);
                result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
                result.Apply();
                return result;
            }
        }

        public class Option
        {
            public Operation Operation { get; set; }

            public GUIContent Icon { get; set; }

            public IHandler<Terrain> Handler { get; set; }

            public Option(Operation operation, GUIContent icon, IHandler<Terrain> handler)
            {
                Operation = operation;
                Icon = icon;
                Handler = handler;
            }
        }
    }
}