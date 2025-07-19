using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Managers.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Animations.ResultScene
{
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private Button backToMenuButton;

        private void Awake()
        {
            backToMenuButton.onClick.AddListener(BackToMenu);
        }

        private void BackToMenu()
        {
            InterfaceManager.Instance.CloseToRoot();
            GameLauncher.Instance.LeaveSession();
        }
    }
}