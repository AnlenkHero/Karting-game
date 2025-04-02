using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Managers.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Screens
{
    public class OptionsScreen : MonoBehaviour
    {
        [SerializeField] private UIScreen optionsScreen;
        [SerializeField] private UIScreen graphicsScreen;
        [SerializeField] private UIScreen audioScreen;
        [SerializeField] private UIScreen userScreen;
        
        [SerializeField] private Button graphicsButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Button userButton;
        [SerializeField] private Button backButton;

        private void OnEnable()
        {
            userButton.interactable = RoomPlayer.Local == null;
        }

        private void Awake()
        {
            graphicsButton.onClick.AddListener(Graphics);
            audioButton.onClick.AddListener(Audio);
            userButton.onClick.AddListener(User);
            backButton.onClick.AddListener(Back);
        }
        
        private void Graphics()
        {
            InterfaceManager.Instance.ShowScreen(graphicsScreen);
        }
        
        private void Audio()
        {
            InterfaceManager.Instance.ShowScreen(audioScreen);
        }
        
        private void User()
        {
            InterfaceManager.Instance.ShowScreen(userScreen);
        }
        
        private void Back()
        {
            InterfaceManager.Instance.CloseActiveScreen();
        }
    }
}