using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class UIScreen : MonoBehaviour
    {
        [SerializeField] private Selectable firstSelected = null;

        /// <summary>
        /// Activates the screen and selects its first UI element.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            if (firstSelected != null)
                firstSelected.Select();
        }

        /// <summary>
        /// Deactivates the screen.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}