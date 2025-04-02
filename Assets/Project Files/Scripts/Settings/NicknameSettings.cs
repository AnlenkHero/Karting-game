using System;
using Kart.Project_Files.Scripts.Fusion;
using TMPro;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Settings
{
    public class NicknameSettings : MonoBehaviour, IUserSettingsOption
    {
        [SerializeField] private TMP_InputField nicknameInputField;
        [SerializeField] private TextMeshProUGUI errorText;

        public event Action OnValidityChanged;

        private void OnEnable()
        {
            nicknameInputField.onValueChanged.AddListener(OnNicknameChanged);
            nicknameInputField.text = ClientInfo.Username;
        }

        private void OnDisable()
        {
            nicknameInputField.onValueChanged.RemoveListener(OnNicknameChanged);
        }

        private void OnNicknameChanged(string newNickname)
        {
            if (IsValid())
            {
                errorText.gameObject.SetActive(false);
            }
            else
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Username must be between 1 and 15 characters.";
            }

            OnValidityChanged?.Invoke();
        }

        public bool IsValid()
        {
            string nickname = nicknameInputField.text;
            return !string.IsNullOrWhiteSpace(nickname) && nickname.Length <= 15;
        }

        public void ApplySetting()
        {
            if (IsValid())
            {
                ClientInfo.Username = nicknameInputField.text;
            }
            else
            {
                errorText.text = "Invalid username.";
            }
        }
    }
}