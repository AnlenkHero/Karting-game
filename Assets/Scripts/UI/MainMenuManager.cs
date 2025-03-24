using System.Threading.Tasks;
using Kart.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private Button matchmakingButton;
        [SerializeField] private Button cancelMatchmakingButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private UIScreen mainMenuScreen;
        private bool _isMatchmakingInProgress;

        private void Awake()
        {
            matchmakingButton.onClick.AddListener(() => Task.Run(StartMatchmaking));
            cancelMatchmakingButton.onClick.AddListener(CancelMatchmaking);
            settingsButton.onClick.AddListener(OpenSettingsMenu);
            InterfaceManager.Instance.SetRootScreen(mainMenuScreen);
        }

        private async Task StartMatchmaking()
        {
            if (_isMatchmakingInProgress) return;
            _isMatchmakingInProgress = true;

            await GameLauncher.Instance.JoinOrCreateLobby();

            matchmakingButton.gameObject.SetActive(false);
            cancelMatchmakingButton.gameObject.SetActive(true);
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
    }
}