using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Kart.Settings
{
    public class GraphicsSettings : MonoBehaviour
    {
        public TMP_Dropdown graphicsDropdown;
        public TMP_Dropdown resolutionDropdown;
        public Toggle postprocessingToggle;
        public Volume volumeProfile;

        private const string GraphicsQualityKey = "GraphicsQuality";
        private const string PostProcessingKey = "PostProcessing";
        private const string ResolutionIndexKey = "ResolutionIndex";

        private Resolution[] resolutions;

        private void Awake()
        {
            graphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            postprocessingToggle.onValueChanged.AddListener(TogglePostProcessing);

            InitGraphicsDropdown();
            InitResolutionDropdown();
            LoadSettings();
        }

        private void InitGraphicsDropdown()
        {
            string[] names = QualitySettings.names;
            graphicsDropdown.ClearOptions();
            graphicsDropdown.AddOptions(new List<string>(names));

            int currentQuality = PlayerPrefs.GetInt(GraphicsQualityKey, QualitySettings.GetQualityLevel());
            graphicsDropdown.value = currentQuality;
        }

        private void InitResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();
            resolutions = Screen.resolutions;
            List<string> options = new List<string>();
            int savedResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, 0);

            for (int i = 0; i < resolutions.Length; i++)
            {
                string resolutionOption = $"{resolutions[i].width} x {resolutions[i].height} @ {resolutions[i].refreshRate}Hz";
                options.Add(resolutionOption);
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = savedResolutionIndex;
        }

        private void SetGraphicsQuality(int value)
        {
            QualitySettings.SetQualityLevel(value);
            PlayerPrefs.SetInt(GraphicsQualityKey, value);
            PlayerPrefs.Save();
        }

        private void SetResolution(int index)
        {
            if (index >= 0 && index < resolutions.Length)
            {
                Resolution selectedResolution = resolutions[index];
                Screen.SetResolution(selectedResolution.width, selectedResolution.height, FullScreenMode.ExclusiveFullScreen, selectedResolution.refreshRateRatio);
                PlayerPrefs.SetInt(ResolutionIndexKey, index);
                PlayerPrefs.Save();
            }
        }

        private void TogglePostProcessing(bool value)
        {
            volumeProfile.gameObject.SetActive(value);
            PlayerPrefs.SetInt(PostProcessingKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            if (PlayerPrefs.HasKey(GraphicsQualityKey))
            {
                int savedQuality = PlayerPrefs.GetInt(GraphicsQualityKey);
                QualitySettings.SetQualityLevel(savedQuality);
                graphicsDropdown.value = savedQuality;
            }

            if (PlayerPrefs.HasKey(PostProcessingKey))
            {
                bool savedPostProcessing = PlayerPrefs.GetInt(PostProcessingKey) == 1;
                postprocessingToggle.isOn = savedPostProcessing;
                volumeProfile.gameObject.SetActive(savedPostProcessing);
            }

            if (PlayerPrefs.HasKey(ResolutionIndexKey))
            {
                int savedResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey);
                if (savedResolutionIndex >= 0 && savedResolutionIndex < resolutions.Length)
                {
                    Resolution selectedResolution = resolutions[savedResolutionIndex];
                    Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen, selectedResolution.refreshRate);
                    resolutionDropdown.value = savedResolutionIndex;
                }
            }
        }
    }
}
