using Managers;

namespace Kart.UI
{
    public class StartGameButton : MainMenuButton
    {
        private void Awake()
        {
            button.onClick.AddListener(DebugButton);
        }

        private void DebugButton()
        {
            //GameLauncher.Instance.GameStarted();
            LevelManager.LoadTrack(2);
        }
    }
}