using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class EscapeMenu : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button leaveButton;

        [SerializeField] private UIScreen escapeMenuScreen;
        [SerializeField] private UIScreen settingsScreen;
        
        private void Awake()
        {
            UIScreen.Focus(escapeMenuScreen);
            resumeButton.onClick.AddListener(Resume);
            settingsButton.onClick.AddListener(Settings);
            leaveButton.onClick.AddListener(Leave);
        }

        private void Resume()
        {
            UIScreen.activeScreen?.Back();
        }

        private void Settings()
        {
            UIScreen.activeScreen?.FocusScreen(settingsScreen);
        }

        private void Leave()
        {
            GameLauncher.Instance.LeaveSession();
        }
    }
}