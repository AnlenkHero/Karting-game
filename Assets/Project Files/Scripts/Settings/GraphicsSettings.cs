using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Settings
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

        private Resolution[] _resolutions;

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

        private void OnEnable()
        {
            int actualMode = (int)Screen.fullScreenMode;
            screenModeDropdown.SetValueWithoutNotify(actualMode);
            screenModeDropdown.RefreshShownValue();
        }

        private void InitGraphicsDropdown()
        {
            graphicsDropdown.ClearOptions();
            graphicsDropdown.AddOptions(QualitySettings.names.ToList());

            int savedQuality = PlayerPrefs.GetInt(GraphicsQualityKey, QualitySettings.GetQualityLevel());
            graphicsDropdown.SetValueWithoutNotify(savedQuality);
            graphicsDropdown.RefreshShownValue();
        }

        private void InitResolutionDropdown()
        {
            resolutionDropdown.ClearOptions();
            _resolutions = Screen.resolutions
                .OrderByDescending(r => r.width * r.height)
                .ThenByDescending(r => r.refreshRateRatio.value)
                .ToArray();

            int savedIndex = PlayerPrefs.GetInt(ResolutionIndexKey, -1);
            if (savedIndex < 0 || savedIndex >= _resolutions.Length)
                savedIndex = 0;

            int maxW = _resolutions.Max(r => r.width).ToString().Length;
            int maxH = _resolutions.Max(r => r.height).ToString().Length;

            var options = _resolutions
                .Select(r =>
                    $"{r.width.ToString().PadRight(maxW)} x {r.height.ToString().PadRight(maxH)}  @ {(int)r.refreshRateRatio.value}Hz")
                .ToList();

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.SetValueWithoutNotify(savedIndex);
            resolutionDropdown.RefreshShownValue();
        }

        private void InitScreenModeDropdown()
        {
            screenModeDropdown.ClearOptions();
            var names = Enum.GetNames(typeof(FullScreenMode)).ToList();
            screenModeDropdown.AddOptions(names);

            int savedMode = PlayerPrefs.GetInt(ScreenModeKey, (int)Screen.fullScreenMode);
            screenModeDropdown.SetValueWithoutNotify(savedMode);
            screenModeDropdown.RefreshShownValue();
        }

        private void SetGraphicsQuality(int idx)
        {
            QualitySettings.SetQualityLevel(idx);
            PlayerPrefs.SetInt(GraphicsQualityKey, idx);
            PlayerPrefs.Save();
        }

        private void SetResolution(int idx)
        {
            if (idx < 0 || idx >= _resolutions.Length) return;
            var res = _resolutions[idx];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
            PlayerPrefs.SetInt(ResolutionIndexKey, idx);
            PlayerPrefs.Save();
        }

        private void SetScreenMode(int idx)
        {
            if (!Enum.IsDefined(typeof(FullScreenMode), idx)) return;
            var mode = (FullScreenMode)idx;
            var res = _resolutions[resolutionDropdown.value];
            Screen.SetResolution(res.width, res.height, mode, res.refreshRateRatio);
            PlayerPrefs.SetInt(ScreenModeKey, idx);
            PlayerPrefs.Save();
        }

        private void TogglePostProcessing(bool enabled)
        {
            volumeProfile.gameObject.SetActive(enabled);
            PlayerPrefs.SetInt(PostProcessingKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            LoadQualitySettings();
            LoadPostProcessingSettings();
            LoadResolutionSettings();
            LoadScreenModeSettings();
        }

        private void LoadScreenModeSettings()
        {
            if (!PlayerPrefs.HasKey(ScreenModeKey)) return;
            int mode = PlayerPrefs.GetInt(ScreenModeKey);
            
            if (!Enum.IsDefined(typeof(FullScreenMode), mode)) return;
            Screen.fullScreenMode = (FullScreenMode)mode;
            screenModeDropdown.SetValueWithoutNotify(mode);
            screenModeDropdown.RefreshShownValue();
        }

        private void LoadResolutionSettings()
        {
            if (!PlayerPrefs.HasKey(ResolutionIndexKey)) return;
            int idx = PlayerPrefs.GetInt(ResolutionIndexKey);
            
            if (idx < 0 || idx >= _resolutions.Length) return;
            var res = _resolutions[idx];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);
            resolutionDropdown.SetValueWithoutNotify(idx);
            resolutionDropdown.RefreshShownValue();
        }

        private void LoadPostProcessingSettings()
        {
            if (!PlayerPrefs.HasKey(PostProcessingKey)) return;
            bool on = PlayerPrefs.GetInt(PostProcessingKey) == 1;
            postprocessingToggle.SetIsOnWithoutNotify(on);
            volumeProfile.gameObject.SetActive(on);
        }

        private void LoadQualitySettings()
        {
            if (!PlayerPrefs.HasKey(GraphicsQualityKey)) return;
            int q = PlayerPrefs.GetInt(GraphicsQualityKey);
            QualitySettings.SetQualityLevel(q);
            graphicsDropdown.SetValueWithoutNotify(q);
            graphicsDropdown.RefreshShownValue();
        }
    }
}