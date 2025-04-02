using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Settings
{
    public class UserSettings : MonoBehaviour
    {
        [SerializeField] private Button globalConfirmButton;
        [SerializeField] private List<MonoBehaviour> settingsPanels;

        private List<IUserSettingsOption> _settingPanels;

        private void Awake()
        {
            _settingPanels = new List<IUserSettingsOption>();
            foreach (var panel in settingsPanels)
            {
                if (panel is IUserSettingsOption settingPanel)
                {
                    _settingPanels.Add(settingPanel);
                    settingPanel.OnValidityChanged += UpdateConfirmButtonState;
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var panel in _settingPanels)
            {
                panel.OnValidityChanged -= UpdateConfirmButtonState;
            }
        }

        private void Start()
        {
            UpdateConfirmButtonState();
            globalConfirmButton.onClick.AddListener(ApplyAllSettings);
        }

        private void UpdateConfirmButtonState()
        {
            bool allValid = true;
            foreach (var panel in _settingPanels)
            {
                if (!panel.IsValid())
                {
                    allValid = false;
                    break;
                }
            }
            globalConfirmButton.interactable = allValid;
        }

        private void ApplyAllSettings()
        {
            foreach (var panel in _settingPanels)
            {
                panel.ApplySetting();
            }
        }
    }
}