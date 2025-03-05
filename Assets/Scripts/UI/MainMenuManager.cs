using Kart.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private Button matchmakingButton;
        [SerializeField] private Button settingsButton;

        private void Awake()
        {
            matchmakingButton.onClick.AddListener(StartMatchmaking);
            settingsButton.onClick.AddListener(OpenSettingsMenu);
        }
        
        private void StartMatchmaking()
        {
            GameLauncher.Instance.JoinOrCreateLobby();
        }
        
        private void OpenSettingsMenu()
        {
            UIScreen.Focus(InterfaceManager.Instance.settingsMenu);
        }
    }
}