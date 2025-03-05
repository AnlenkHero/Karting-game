using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class OptionsPanel : MonoBehaviour
    {
        [SerializeField] private UIScreen optionsScreen;
        [SerializeField] private UIScreen graphicsScreen;
        [SerializeField] private UIScreen audioScreen;
        [SerializeField] private UIScreen userScreen;
        
        [SerializeField] private Button graphicsButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Button userButton;
        [SerializeField] private Button backButton;
        
        private void Awake()
        {
            graphicsButton.onClick.AddListener(Graphics);
            audioButton.onClick.AddListener(Audio);
            userButton.onClick.AddListener(User);
            backButton.onClick.AddListener(Back);
        }
        
        private void Graphics()
        {
            optionsScreen.FocusScreen(graphicsScreen);
        }
        
        private void Audio()
        {
            optionsScreen.FocusScreen(audioScreen);
        }
        
        private void User()
        {
            optionsScreen.FocusScreen(userScreen);
        }
        
        private void Back()
        {
            optionsScreen.Back();
        }
    }
}