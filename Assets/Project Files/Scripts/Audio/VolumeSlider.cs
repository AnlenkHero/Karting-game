using Kart.Project_Files.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Audio
{
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        public string mixerParameter;
        public string mixerGroup;
        private float _lastVal;

        private void OnEnable()
        {
            _lastVal = slider.value = PlayerPrefs.GetFloat(mixerParameter, 0.75f);
            slider.onValueChanged.AddListener((val) =>
            {
                if (Mathf.Approximately(Mathf.Round(val * 10), Mathf.Round(_lastVal * 10))) return;
                AudioManager.Play("hoverUI", mixerGroup);
                _lastVal = val;
                AudioManager.SetVolume(mixerParameter, val);
            });
        }
    }
}