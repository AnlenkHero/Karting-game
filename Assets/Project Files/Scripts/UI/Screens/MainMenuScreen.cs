using System;
using Kart.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private Button matchmakingButton;
        [SerializeField] private Button cancelMatchmakingButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [SerializeField] private UIScreen mainMenuScreen;
        private bool _isMatchmakingInProgress;

        private void Awake()
        {
            matchmakingButton.onClick.AddListener(StartMatchmaking);
            cancelMatchmakingButton.onClick.AddListener(CancelMatchmaking);
            settingsButton.onClick.AddListener(OpenSettingsMenu);
            quitButton.onClick.AddListener(QuitGame);
            InterfaceManager.Instance.SetRootScreen(mainMenuScreen);
        }

        private async void StartMatchmaking()
        {
            try
            {
                if (_isMatchmakingInProgress) return;
                _isMatchmakingInProgress = true;

                await GameLauncher.Instance.JoinOrCreateLobby();

                matchmakingButton.gameObject.SetActive(false);
                cancelMatchmakingButton.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to start matchmaking: " + e.Message);
            }
        }

        private void CancelMatchmaking()
        {
            if (!_isMatchmakingInProgress) return;

            GameLauncher.Instance.LeaveSession();
            _isMatchmakingInProgress = false;

            cancelMatchmakingButton.gameObject.SetActive(false);
            matchmakingButton.gameObject.SetActive(true);
        }

        private void OpenSettingsMenu()
        {
            InterfaceManager.Instance.ShowScreen(InterfaceManager.Instance.SettingsMenu);
        }

        private void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}