using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class EscapeMenu : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button leaveButton;

        [SerializeField] private GameObject settingsPanel;

        private void Awake()
        {
            resumeButton.onClick.AddListener(Resume);
            settingsButton.onClick.AddListener(Settings);
            leaveButton.onClick.AddListener(Leave);
        }

        private void Resume()
        {
            gameObject.SetActive(false);
        }

        private void Settings()
        {
            settingsPanel.SetActive(true);
        }

        private void Leave()
        {
            GameLauncher.Instance.LeaveSession();
        }
    }
}