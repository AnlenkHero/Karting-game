using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Animations.Dropdown
{
    public class DropdownOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image textBackground;
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite disabledSprite;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField ] private Color normalColor;
        [SerializeField] private Color hoverColor;


        public void OnPointerEnter(PointerEventData eventData)
        {
            text.color = hoverColor;
            textBackground.sprite = disabledSprite;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            text.color = normalColor;
            textBackground.sprite = normalSprite;
        }
    }
}