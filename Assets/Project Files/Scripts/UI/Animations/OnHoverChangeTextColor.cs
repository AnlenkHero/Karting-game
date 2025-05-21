using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kart.Project_Files.Scripts.UI.Animations
{
    public class OnHoverChangeTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private TextMeshProUGUI textTarget;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color hoverColor;
        
        [SerializeField] private bool shouldFadeColorIfSelected;
        [SerializeField] private bool shouldChangeColorIfSelected;
        [SerializeField] private Color selectedHoverColor;
        [SerializeField] private Color selectedNormalColor;

        [SerializeField] private bool shouldChangeColorIfPressed;
        [SerializeField] private Color pressedColor;
        [SerializeField] private Color releasedColor;

        private void OnDisable()
        {
            textTarget.color = normalColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (shouldChangeColorIfSelected && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                textTarget.color = selectedHoverColor;
            }
            else
            {
                textTarget.color = hoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!shouldFadeColorIfSelected && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                textTarget.color = shouldChangeColorIfSelected ? selectedHoverColor : hoverColor;
            }
            else
            {
                textTarget.color = shouldFadeColorIfSelected ? selectedNormalColor : normalColor;
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (shouldChangeColorIfSelected)
            {
                textTarget.color = selectedHoverColor;
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (shouldChangeColorIfSelected)
            {
                textTarget.color = normalColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (shouldChangeColorIfPressed)
            {
                textTarget.color = pressedColor;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (shouldChangeColorIfPressed)
            {
                textTarget.color = releasedColor;
            }
        }
    }
}

