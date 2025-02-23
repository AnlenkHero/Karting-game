using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class MainMenuButton : MonoBehaviour
    {
        public Button button;
        public Image image;

        private void Awake()
        {
            button.onClick.AddListener(DebugButton);
        }

        private void DebugButton()
        {
            //GameLauncher.Instance.JoinOrCreateLobby();
        }
    }
}