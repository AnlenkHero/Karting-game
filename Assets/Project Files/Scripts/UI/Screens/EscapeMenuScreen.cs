using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Managers.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Screens
{
    public class EscapeMenuScreen : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button leaveButton;

        [SerializeField] private UIScreen escapeMenuScreen;
        [SerializeField] private UIScreen settingsScreen;
        
        private void Awake()
        {
            resumeButton.onClick.AddListener(Resume);
            settingsButton.onClick.AddListener(Settings);
            leaveButton.onClick.AddListener(Leave);
        }
        

        private void Resume()
        {
            InterfaceManager.Instance.CloseActiveScreen();
        }

        private void Settings()
        {
            InterfaceManager.Instance.ShowScreen(InterfaceManager.Instance.SettingsMenu);
        }

        private void Leave()
        {
            GameLauncher.Instance.LeaveSession();
        }
    }
}