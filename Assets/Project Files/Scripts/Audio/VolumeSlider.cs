using System;
using Kart.Project_Files.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Audio
{
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private float sliderSoundThresholdTime = 0.1f;
        public string mixerParameter;
        public string mixerGroup;
        private float _lastVal;
        private float _currentSliderSoundTime;

        private void Update()
        {
            _currentSliderSoundTime += Time.deltaTime;
        }

        private void OnEnable()
        {
            _lastVal = slider.value = PlayerPrefs.GetFloat(mixerParameter, 0.75f);
            slider.onValueChanged.AddListener((val) =>
            {
                if (Mathf.Approximately(val, _lastVal)) return;
                
                _lastVal = val;
                AudioManager.SetVolume(mixerParameter, val);
                
                if (!(_currentSliderSoundTime >= sliderSoundThresholdTime)) return;
                AudioManager.Play("hoverUI", mixerGroup);
                _currentSliderSoundTime = 0;
            });
        }
    }
}