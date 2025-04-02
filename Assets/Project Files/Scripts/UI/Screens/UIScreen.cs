using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Screens
{
    public class UIScreen : MonoBehaviour
    {
        [SerializeField] private Selectable firstSelected;
        
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