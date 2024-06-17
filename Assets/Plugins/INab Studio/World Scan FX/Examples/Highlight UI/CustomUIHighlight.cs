using System.Collections;
using TMPro;
using UnityEngine;

namespace INab.WorldScanFX
{
    [ExecuteAlways]
    public class CustomUIHighlight : MonoBehaviour
    {
        // Reference to the UI component to be highlighted
        public GameObject uiComponent;
        // Reference to the TextMeshPro component of the UI
        public TextMeshProUGUI uiText;
        // Duration of the highlight effect
        public float highlightDuration = 3f;
        // Duration of scaling animation
        public float scaleDuration = 0.5f;
        // Offset on the Y-axis for the UI component
        public float offsetY = 1f;
        // Adjustment factor for scale based on distance
        public float scaleAdjustment = 10;

        // Reference to the player's transform
        public Transform playerTransform;
        // Reference to the player's camera
        public Camera playerCamera;

        // Remaining time for the highlight effect
        private float timeLeft = 0;
        // Current scale of the UI component
        private float currentScale = 1;
        // Flag to indicate if scaling animation is in progress
        private bool isCurrentlyScaling = false;
        // Flag to indicate if the highlight effect is active
        private bool isEffectActive = false;

        private void Start()
        {
            // Deactivate the UI component initially
            uiComponent.SetActive(false);
        }

        // Method to start the highlight effect
        public void StartEffect()
        {
            if (isEffectActive == true)
            {
                // Restart the effect if already active
                timeLeft = highlightDuration;
            }
            else
            {
                // Activate the effect
                isEffectActive = true;
                uiComponent.SetActive(true);
                timeLeft = highlightDuration;
                // Start scaling animation
                StartCoroutine(ScaleUI(true));
            }
        }

        // Method to stop the highlight effect
        public void StopEffect()
        {
            isEffectActive = false;
            // Start scaling animation to hide the UI
            StartCoroutine(ScaleUI(false));
        }

        // Coroutine for scaling the UI component
        private IEnumerator ScaleUI(bool show)
        {
            isCurrentlyScaling = true;

            float elapsedTime = 0f;
            while (elapsedTime < scaleDuration)
            {
                elapsedTime += Time.deltaTime;
                float effectTime = elapsedTime / scaleDuration;
                float value = show ? Mathf.SmoothStep(0, currentScale, effectTime) : Mathf.SmoothStep(currentScale, 0, effectTime);
                uiComponent.transform.localScale = new Vector3(value, value, value);
                yield return null;
            }

            isCurrentlyScaling = false;

            if (!show)
                uiComponent.SetActive(false);
        }

        // Update is called once per frame
        public void Update()
        {
            if (isEffectActive)
                timeLeft -= Time.deltaTime;

            if (timeLeft < 0 && Application.isPlaying && isEffectActive)
            {
                StopEffect();
            }
            else
            {
                // Calculate distance between player and UI
                float distance = Vector3.Distance(playerTransform.position, transform.position);
                // Convert distance to text and display on UI
                string distanceText = Mathf.CeilToInt(distance) + "M";
                uiText.text = distanceText;

                // Calculate new position for UI based on player's camera
                // check if position is behind camera
                bool isBehindCamera = playerCamera.WorldToScreenPoint(transform.position).z < 0;
                var rect = uiComponent.GetComponent<RectTransform>();

                if (!isBehindCamera)
                {
                    var newPosition = playerCamera.WorldToScreenPoint(transform.position + new Vector3(0, offsetY, 0));
                    rect.position = newPosition;
                }

                // Adjust scale of UI based on distance
                currentScale = Mathf.Clamp01(scaleAdjustment / distance);
                if (!isCurrentlyScaling)
                    rect.localScale = new Vector3(currentScale, currentScale, currentScale);
            }
        }
    }
}
