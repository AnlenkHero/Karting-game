using System;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Managers.Interface;
using Kart.Project_Files.Scripts.UI.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Screens
{
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private SteeringButtonData matchmakingButton;
        [SerializeField] private SteeringButtonData cancelMatchmakingButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private UIScreen mainMenuScreen;
        private bool _isMatchmakingInProgress;

        private void Awake()
        {
            matchmakingButton.button.onClick.AddListener(StartMatchmaking);
            cancelMatchmakingButton.button.onClick.AddListener(CancelMatchmaking);
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

                await GameLauncher.Instance.JoinOrCreateMatchmakingLobby();

                matchmakingButton.isVisible = false;
                cancelMatchmakingButton.isVisible = true;
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

            cancelMatchmakingButton.isVisible = false;
            matchmakingButton.isVisible = true;
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