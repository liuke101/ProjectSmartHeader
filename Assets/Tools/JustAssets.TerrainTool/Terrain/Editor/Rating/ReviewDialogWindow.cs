using System;
using UnityEditor;
using UnityEngine;

namespace JustAssets.TerrainUtility.Rating
{
    public class ReviewDialogWindow : EditorWindow
    {
        private int _selectedStars;
        private int _hoveredStars = 0;
        private const int TotalStars = 5;

        // Base64 strings for the empty and filled star images.
        private const string _emptyStarBase64 = "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";
        private const string _filledStarBase64 = "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";

        private Texture2D _emptyStarTexture;
        private Texture2D _filledStarTexture;

        private Rect _starsArea;
        private Rect _dialogArea;

        private const int DialogWidth = 400;
        private const int DialogHeight = 140;
        
        public static void ShowWindow()
        {
            Rect rect = new Rect(Screen.width / 2 - DialogWidth / 2, Screen.height / 2 - DialogHeight / 2, DialogWidth, DialogHeight);
            GetWindowWithRect<ReviewDialogWindow>(rect, true, "Review " + AssetReviewHandler.ProductName);
        
        }

        private void OnEnable()
        {
            // Decode the Base64 strings and create the textures.
            _emptyStarTexture = LoadTextureFromBase64(_emptyStarBase64);
            _filledStarTexture = LoadTextureFromBase64(_filledStarBase64);

            // Calculate the center position for the stars area.
            _dialogArea = new Rect(0, 0, DialogWidth, DialogHeight);
            _starsArea = new Rect(DialogWidth / 2 - _emptyStarTexture.width * TotalStars / 2, 30, _emptyStarTexture.width * TotalStars, _emptyStarTexture.height);

            AssetReviewHandler.RemindMeLater();
        }
    
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            // Check if the dialog was not shown during this Unity session.
            if (AssetReviewHandler.ShouldShowReviewDialog())
            {
                EditorApplication.delayCall += ShowWindow;
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(_dialogArea);
            GUILayout.BeginVertical();
            GUILayout.Space(10);
        
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Do you like '{AssetReviewHandler.ProductName}'? Please give us a rating:", EditorStyles.largeLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        
            // Draw stars area centered within the window.
            Rect starsRect = new Rect(DialogWidth / 2 - _starsArea.width / 2, 45, _starsArea.width, _starsArea.height);
            GUILayout.BeginArea(starsRect);
            GUILayout.BeginHorizontal();

            for (int i = 0; i < TotalStars; i++)
            {
                Rect starRect = GUILayoutUtility.GetRect(_emptyStarTexture.width, _emptyStarTexture.height, GUILayout.Width(_emptyStarTexture.width), GUILayout.Height(_emptyStarTexture.height));

                // Check if the mouse is over the star and update the _hoveredStars accordingly.
                bool isHovered = starRect.Contains(Event.current.mousePosition);
                if (isHovered)
                {
                    _hoveredStars = i + 1;
                }

                if (Event.current.type == EventType.Repaint)
                {
                    // Draw the star texture based on the current hovering state.
                    GUI.DrawTexture(starRect, i < _hoveredStars ? _filledStarTexture : _emptyStarTexture);
                }

                // Check for a click event to update the _selectedStars with the _hoveredStars.
                if (Event.current.type == EventType.MouseUp && isHovered)
                {
                    _selectedStars = _hoveredStars;
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            bool close = false;

            if (GUILayout.Button("Contact Support") || _selectedStars >= 1 && _selectedStars <= 3)
            {
                AssetReviewHandler.ContactSupport();
                close = true;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Ask Me Later"))
            {
                AssetReviewHandler.RemindMeLater();
                close = true;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();
            GUILayout.EndArea();

            // Call ReviewAsset method when the user rates the asset with 4-5 stars.
            if (_selectedStars >= 4 && _selectedStars <= 5)
            {
                AssetReviewHandler.ReviewAsset();
                close = true;
            }

            if (close)
            {
                Close();
                return;
            }
        
            Repaint();
            EditorUtility.SetDirty(this);
        }
        private Texture2D LoadTextureFromBase64(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                return null;
            }

            try
            {
                byte[] imageData = Convert.FromBase64String(base64);
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(imageData);
                return texture;
            }
            catch (Exception ex)
            {
                Debug.LogError("Error loading texture from Base64: " + ex.Message);
                return null;
            }
        }
    }
}