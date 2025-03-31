using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class UIScreen : MonoBehaviour
    {
        [SerializeField] private Selectable firstSelected = null;
        
        public void Show()
        {
            gameObject.SetActive(true);
            if (firstSelected != null)
                firstSelected.Select();
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}