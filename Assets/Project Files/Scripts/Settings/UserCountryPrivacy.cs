using System;
using Kart.Project_Files.Scripts.Fusion;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Settings
{
    public class UserCountryPrivacy : MonoBehaviour, IUserSettingsOption
    {
        [SerializeField] private Toggle privacyToggle;
        private bool _isPrivacyEnabled;

        private void OnEnable()
        {
            privacyToggle.onValueChanged.AddListener(OnPrivacyToggleChanged);
            privacyToggle.isOn = ClientInfo.CountryPrivacy;
            _isPrivacyEnabled = privacyToggle.isOn;
        }

        private void OnDisable()
        {
            privacyToggle.onValueChanged.RemoveListener(OnPrivacyToggleChanged);
        }

        private void OnPrivacyToggleChanged(bool isOn)
        {
            _isPrivacyEnabled = isOn;
            OnValidityChanged?.Invoke();
        }

        public bool IsValid()
        {
            return true;
        }

        public void ApplySetting()
        {
            ClientInfo.CountryPrivacy = _isPrivacyEnabled;
        }

        public event Action OnValidityChanged;
    }
}