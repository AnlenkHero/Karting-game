using Fusion;
using TMPro;

namespace Kart.UI
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