using Kart.Project_Files.Scripts.Helpers;
using Kart.Project_Files.Scripts.UI.Screens;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Managers.Interface
{
    public class InterfaceManager : MonoBehaviour
    {
        public static InterfaceManager Instance => Singleton<InterfaceManager>.Instance;

        [SerializeField] private InterfaceScreenHandler screenHandler;
        [SerializeField] private GameObject loadingScreen;
        public InterfaceInputHandler inputHandler;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void ShowScreen(UIScreen screen)
        {
            if (screenHandler != null)
                screenHandler.ShowScreen(screen);
        }

        public void CloseActiveScreen()
        {
            if (screenHandler != null)
                screenHandler.CloseActiveScreen();
        }

        public void CloseToRoot()
        {
            if (screenHandler != null)
                screenHandler.CloseToRoot();
        }

        public void SetRootScreen(UIScreen screen)
        {
            screenHandler.SetRootScreen(screen);
        }
        
        public void ShowLoadingScreen(bool show)
        {
            if (loadingScreen != null)
                loadingScreen.SetActive(show);
        }

        public UIScreen ActiveScreen => screenHandler != null ? screenHandler.ActiveScreen : null;
        public UIScreen RootScreen => screenHandler != null ? screenHandler.RootScreen : null;
        public UIScreen EscapeMenuScreen => screenHandler != null ? screenHandler.EscapeMenuScreen : null;

        public UIScreen SettingsMenu => screenHandler != null ? screenHandler.SettingsMenuScreen : null;
    }
}