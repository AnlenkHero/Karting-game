using System;
using Kart.Helpers;
using Kart.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kart.Managers
{
    public class InterfaceManager : MonoBehaviour
    {
        public UIScreen pauseMenu;
        public UIScreen settingsMenu;

        public static InterfaceManager Instance => Singleton<InterfaceManager>.Instance;

        private void OnEnable()
        {
            DontDestroyOnLoad(this);
        }

        public void OpenPauseMenu()
        {
            if (UIScreen.activeScreen != pauseMenu)
            {
                UIScreen.Focus(pauseMenu);
            }
        }
        
        public void OpenSettingsMenu()
        {
            UIScreen.Focus(settingsMenu);
        }
    }
}