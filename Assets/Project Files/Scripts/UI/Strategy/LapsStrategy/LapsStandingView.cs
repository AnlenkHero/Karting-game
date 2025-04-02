using Fusion;
using TMPro;

namespace Kart.Project_Files.Scripts.UI.Strategy.LapsStrategy
{
    public class LapsStandingView : NetworkBehaviour
    {
        public TextMeshProUGUI Text;

        public void SetText(string text)
        {
            Text.text = text;
        }
    }
}