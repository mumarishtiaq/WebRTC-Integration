using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Krivodeling.UI.Effects
{
    public class UIBlur : MonoBehaviour
    {
        public Color Color { get => _color; set { _color = value; UpdateColor(); } }
        public float Intensity { get => _intensity; set { _intensity = Mathf.Clamp01(value); UpdateIntensity(); } }
        public float Multiplier { get => _multiplier; set { _multiplier = Mathf.Clamp01(value); UpdateMultiplier(); } }
        public UnityEvent OnBeginBlur { get => _onBeginBlur; set => _onBeginBlur = value; }
        public UnityEvent OnEndBlur { get => _onEndBlur; set => _onEndBlur = value; }
        public BlurChangedEvent OnBlurChanged { get => _onBlurChanged; set => _onBlurChanged = value; }

        [SerializeField]
        private Color _color = Color.white;
        [SerializeField, Range(0f, 1f)]
        private float _intensity;
        [SerializeField, Range(0f, 1f)]
        private float _multiplier = 0.15f;
        [SerializeField]
        private UnityEvent _onBeginBlur;
        [SerializeField]
        private UnityEvent _onEndBlur;
        [SerializeField]
        private BlurChangedEvent _onBlurChanged;

        private Material _material;
        private int _colorId;
        private int _intensityId;
        private int _multiplierId;

        public void UpdateBlur()
        {
            SetBlur(Color, Intensity, Multiplier);
        }

        public void SetBlur(Color color, float intensity, float multiplier)
        {
            Color = color;
            Intensity = intensity;
            Multiplier = multiplier;
        }

        public void BeginBlur(float speed)
        {
            StopAllCoroutines();
            StartCoroutine(BeginBlurCoroutine(speed));
        }

        public void EndBlur(float speed)
        {
            StopAllCoroutines();
            StartCoroutine(EndBlurCoroutine(speed));
        }

        private void Start()
        {
            SetComponents();
            SetBlur(Color, Intensity, _multiplier);
        }

        private void SetComponents()
        {
            _material = FindMaterial();
            _colorId = Shader.PropertyToID("_Color");
            _intensityId = Shader.PropertyToID("_Intensity");
            _multiplierId = Shader.PropertyToID("_Multiplier");
        }

        private Material FindMaterial()
        {
            Material material = GetComponent<Image>().material;

            if (material == null)
                material = GetComponent<Renderer>().material;

            if (material == null)
                throw new NullReferenceException("Material not found");

            return material;
        }

        private void UpdateColor()
        {
            _material.SetColor(_colorId, Color);
        }

        private void UpdateIntensity()
        {
            _material.SetFloat(_intensityId, Intensity);
        }

        private void UpdateMultiplier()
        {
            _material.SetFloat(_multiplierId, Multiplier);
        }


        private IEnumerator BeginBlurCoroutine(float speed)
        {
            OnBeginBlur?.Invoke();

            while (Intensity < 1f)
            {
                Intensity += speed * Time.unscaledDeltaTime; // Unscaled time use kiya taake UI work kare
                UpdateIntensity();
                OnBlurChanged.Invoke(Intensity);
                yield return null;
            }

            PauseGame(); // Blur ke saath game pause hojai
        }


        public void PauseGame()
        {
            Time.timeScale = 0; // Game freeze
            AudioListener.pause = true; // Audio bhi pause hojai
        }

        public void ResumeGame()
        {
            Time.timeScale = 1; // Game resume
            AudioListener.pause = false;
        }


        private IEnumerator EndBlurCoroutine(float speed)
        {
            while (Intensity > 0f)
            {
                Intensity -= speed * Time.unscaledDeltaTime; // Unscaled time use kiya
                UpdateIntensity();
                OnBlurChanged.Invoke(Intensity);
                yield return null;
            }

            OnEndBlur?.Invoke();
            ResumeGame(); // Blur end hone per game resume hojai
        }


        [Serializable]
        public class BlurChangedEvent : UnityEvent<float> { }

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateBlurInEditor();
        }

        private void UpdateBlurInEditor()
        {
            Material material = FindMaterial();

            material.SetColor("_Color", Color);
            material.SetFloat("_Intensity", Intensity);
            material.SetFloat("_Multiplier", Multiplier);

            EditorUtility.SetDirty(material);
        }
#endif
        #endregion
    }
}
