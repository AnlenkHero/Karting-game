using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kart.Project_Files.Scripts.UI.Animations
{
    public class OnHoverChangeTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Color normalColor;
        [SerializeField] private Color hoverColor;
        [SerializeField] private TextMeshProUGUI textTarget;
        [SerializeField] private bool shouldFadeColorIfSelected;

        public void OnPointerEnter(PointerEventData eventData)
        {
            textTarget.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!shouldFadeColorIfSelected && EventSystem.current.currentSelectedGameObject == gameObject)
                textTarget.color = hoverColor;
            else
                textTarget.color = normalColor;
        }
    }
}