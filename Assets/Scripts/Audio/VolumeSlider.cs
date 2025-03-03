using Kart.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Audio
{
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        public string mixerParameter;
        public string mixerGroup;
        private float lastVal;

        private void OnEnable()
        {
            lastVal = slider.value = PlayerPrefs.GetFloat(mixerParameter, 0.75f);
            slider.onValueChanged.AddListener((val) =>
            {
                if (Mathf.Approximately(Mathf.Round(val * 10), Mathf.Round(lastVal * 10))) return;
                AudioManager.Play("hoverUI", mixerGroup);
                lastVal = val;
                AudioManager.SetVolume(mixerParameter, val);
            });
        }
    }
}