using System;
using UnityEditor;
using UnityEngine;

namespace JustAssets.TerrainUtility.Rating
{
    public class AssetReviewHandler
    {
        public const double REMIND_ME_LATER_DAYS = 2;
        
        public static string ProductName { get; } = TerrainUtility.PRODUCT_NAME;

        private static readonly string AskMeLaterTimeKey = $"AskMeLaterTime_{ProductName}";

        private static readonly string ReviewDoneKey = $"ReviewDone_{ProductName}";

        private static readonly string SupportPageURL = TerrainUtility.SUPPORT_LINK;

        private static readonly string AssetStoreURL = "https://assetstore.unity.com/packages/tools/utilities/terrain-split-and-merge-utility-209845";

        public static void ReviewAsset()
        {
            // Open the asset store page for the user to review the asset.
            OpenURL(AssetStoreURL);

            // Set the flag to indicate that the review dialog has been shown on this machine.
            EditorPrefs.SetBool(ReviewDoneKey, true);
        }

        public static void ContactSupport()
        {
            // Open the support page for the user to get assistance.
            OpenURL(SupportPageURL);
        }

        private static void OpenURL(string url)
        {
            // Check if the URL is valid.
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("Invalid URL.");
                return;
            }

            // Open the URL in the default web browser.
            Application.OpenURL(url);
        }

        public static bool ShouldShowReviewDialog()
        {
            if (EditorPrefs.GetBool(ReviewDoneKey, false))
                return false;
        
            if (IsItTimeToRemind())
                return true;

            return false;
        }

        public static void RemindMeLater()
        {
            // Delay the dialog by two days.
            var askMeLaterTime = DateTime.Now.AddDays(REMIND_ME_LATER_DAYS);
            EditorPrefs.SetString(AskMeLaterTimeKey, askMeLaterTime.ToString());
        }

        private static bool IsItTimeToRemind()
        {
            // Load the saved time for "Ask Me Later" option.
            string savedTime = EditorPrefs.GetString(AskMeLaterTimeKey, string.Empty);
            if (!string.IsNullOrEmpty(savedTime))
            {
                return DateTime.Parse(savedTime) >= DateTime.Now;
            }

            // Do not show the dialog after installation. Treat it as if automatically clicking remind me later.
            RemindMeLater();
            return false;
        }
    }
}
