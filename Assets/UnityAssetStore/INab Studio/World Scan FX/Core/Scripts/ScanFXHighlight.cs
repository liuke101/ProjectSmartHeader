using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace INab.WorldScanFX
{
    public class ScanFXHighlight : MonoBehaviour
    {
        [Tooltip("Renderers to be highlighted.")]
        public List<Renderer> renderers = new List<Renderer>();

        [Tooltip("Duration of the highlight effect, in seconds.")]
        public float highlightDuration = 5f;

        [Tooltip("Animation curve for the highlight effect, defining its intensity over time.")]
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Tooltip("Events triggered when the highlight effect starts.")]
        public UnityEvent highlightEvent;

        // Used internally for managing the highlight effect coroutine.
        private IEnumerator enumerator;

        // Used internally for applying changes to the renderer materials without affecting shared materials.
        private MaterialPropertyBlock materialPropertyBlock;

        private bool effectIsPlaying = false;

        private bool alreadyScanned = false;
        public bool AlreadyScanned
        {
            get
            {
                return alreadyScanned;
            }
            set
            {
                alreadyScanned = value;
            }
        }

        #region PrivateMethods

        private IEnumerator EffectEnumerator()
        {
            float value;
            float elapsedTime = 0f;
            effectIsPlaying = true;

            if (materialPropertyBlock == null) { materialPropertyBlock = new MaterialPropertyBlock(); }

            while (elapsedTime < highlightDuration)
            {
                elapsedTime += Time.deltaTime;

                float effectTime = elapsedTime / highlightDuration;
                value = curve.Evaluate(effectTime);

                UpdateHighlightValue(value);

                yield return null;
            }

            effectIsPlaying = false;
        }

        private void UpdateHighlightValue(float value)
        {
            foreach (var item in renderers)
            {
                materialPropertyBlock.SetFloat("_HighlightValue", value);
                item.SetPropertyBlock(materialPropertyBlock);
            }
        }

        private void OnEnable()
        {
            materialPropertyBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            effectIsPlaying = false;
            UpdateHighlightValue(0);
        }

        #endregion

        #region PublicMethods

        /// <summary>
        /// Play the highlight effect.
        /// </summary>
        public virtual void PlayEffect()
        {
            if(highlightEvent != null) highlightEvent.Invoke();

            alreadyScanned = true;

            // If the effect is playing, stop coroutine
            if (effectIsPlaying == true)
            {
                if (enumerator != null) StopCoroutine(enumerator);
            }

            enumerator = EffectEnumerator();
            StartCoroutine(enumerator);
        }

        /// <summary>
        /// Find renderers using GetComponentsInChildren.
        /// </summary>
        public void FindRenderersInChildren()
        {
            renderers.Clear();
            foreach (var item in GetComponentsInChildren<Renderer>())
            {
                renderers.Add(item);
            }
        }

        /// <summary>
        /// Find renderers using GetComponents.
        /// </summary>
        public void FindRenderers()
        {
            renderers.Clear();
            foreach (var item in GetComponents<Renderer>())
            {
                renderers.Add(item);
            }
        }

        #endregion
    }
}
