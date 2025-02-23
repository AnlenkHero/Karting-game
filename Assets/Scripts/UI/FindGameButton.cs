using System.Threading.Tasks;
using Managers;

namespace Kart.UI
{
    public class FindGameButton : MainMenuButton
    {
        private void Awake()
        {
            button.onClick.AddListener(DebugButton);
        }

        private void DebugButton()
        {
            GameLauncher.Instance.JoinOrCreateLobby();
        }
    }
}