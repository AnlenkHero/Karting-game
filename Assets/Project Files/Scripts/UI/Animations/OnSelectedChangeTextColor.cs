using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kart.Project_Files.Scripts.UI.Animations
{
    public class OnSelectedChangeTextColor : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private TextMeshProUGUI textTarget;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;

        private void OnDisable()
        {
            textTarget.color = normalColor;
        }

        public void OnSelect(BaseEventData eventData)
        {
            textTarget.color = selectedColor;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            textTarget.color = normalColor;
        }
    }
}