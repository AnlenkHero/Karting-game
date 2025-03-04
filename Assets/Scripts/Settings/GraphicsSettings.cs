using System;
using System.Collections.Generic;
using System.Linq;
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
        public TMP_Dropdown screenModeDropdown;
        public Toggle postprocessingToggle;
        public Volume volumeProfile;

        private const string GraphicsQualityKey = "GraphicsQuality";
        private const string PostProcessingKey = "PostProcessing";
        private const string ResolutionIndexKey = "ResolutionIndex";
        private const string ScreenModeKey = "ScreenMode";

        private Resolution[] resolutions;

        private void Awake()
        {
            graphicsDropdown.onValueChanged.AddListener(SetGraphicsQuality);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
            screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
            postprocessingToggle.onValueChanged.AddListener(TogglePostProcessing);

            InitGraphicsDropdown();
            InitResolutionDropdown();
            InitScreenModeDropdown();
            LoadSettings();
        }

        private void InitGraphicsDropdown()
        {
            graphicsDropdown.ClearOptions();
            graphicsDropdown.AddOptions(QualitySettings.names.ToList());

            int currentQuality = PlayerPrefs.GetInt(GraphicsQualityKey, QualitySettings.GetQualityLevel());
            graphicsDropdown.value = currentQuality;
        }

        private void InitResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();
            resolutions = Screen.resolutions
                .OrderByDescending(r => r.width * r.height)
                .ThenByDescending(r => r.refreshRateRatio.value)
                .ToArray();

            int savedResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, -1);
            if (savedResolutionIndex == -1)
            {
                savedResolutionIndex = 0;
                PlayerPrefs.SetInt(ResolutionIndexKey, savedResolutionIndex);
                PlayerPrefs.Save();
            }

            int maxWidth = resolutions.Max(r => r.width).ToString().Length;
            int maxHeight = resolutions.Max(r => r.height).ToString().Length;

            List<string> options = resolutions
                .Select(r => 
                    $"{r.width.ToString().PadRight(maxWidth)} x {r.height.ToString().PadRight(maxHeight)}  @ {(int)r.refreshRateRatio.value}Hz")
                .ToList();

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = savedResolutionIndex;
        }



        private void InitScreenModeDropdown()
        {
            screenModeDropdown.ClearOptions();
            List<string> options = Enum.GetNames(typeof(FullScreenMode)).ToList();

            screenModeDropdown.AddOptions(options);
            int savedMode = PlayerPrefs.GetInt(ScreenModeKey, (int)Screen.fullScreenMode);
            screenModeDropdown.value = savedMode;
        }

        private void SetGraphicsQuality(int value)
        {
            QualitySettings.SetQualityLevel(value);
            PlayerPrefs.SetInt(GraphicsQualityKey, value);
            PlayerPrefs.Save();
        }

        private void SetResolution(int index)
        {
            if (index < 0 || index >= resolutions.Length) return;

            Resolution selectedResolution = resolutions[index];
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreenMode, selectedResolution.refreshRateRatio);
            PlayerPrefs.SetInt(ResolutionIndexKey, index);
            PlayerPrefs.Save();
        }

        private void SetScreenMode(int index)
        {
            if (!Enum.IsDefined(typeof(FullScreenMode), index)) return;

            FullScreenMode mode = (FullScreenMode)index;
            Resolution selectedResolution = resolutions[resolutionDropdown.value];

            Screen.SetResolution(selectedResolution.width, selectedResolution.height, mode, selectedResolution.refreshRateRatio);
            PlayerPrefs.SetInt(ScreenModeKey, index);
            PlayerPrefs.Save();
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
                    Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreenMode, selectedResolution.refreshRateRatio);
                    resolutionDropdown.value = savedResolutionIndex;
                }
            }

            if (!PlayerPrefs.HasKey(ScreenModeKey)) return;
            int savedMode = PlayerPrefs.GetInt(ScreenModeKey);
            if (!Enum.IsDefined(typeof(FullScreenMode), savedMode)) return;
            Screen.fullScreenMode = (FullScreenMode)savedMode;
            screenModeDropdown.value = savedMode;
        }
    }
}
