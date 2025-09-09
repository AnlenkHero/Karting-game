using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.Managers.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Actions
{
    public class ReturnButton : MonoBehaviour
    {
        [SerializeField] private Button returnButton;

        private void Awake()
        {
            returnButton.onClick.AddListener(Return);
        }

        private void Return()
        {
            var interfaceManager = InterfaceManager.Instance;
            interfaceManager.CloseActiveScreen();

            var kart = KartController.LocalKartController;
            bool isNowRoot = interfaceManager.ActiveScreen == interfaceManager.RootScreen;
            var gameState = GameManager.Instance?.CurrentGameState;

            if (kart != null && isNowRoot && gameState >= GameState.Running)
                kart.canDrive = true;
        }
    }
}