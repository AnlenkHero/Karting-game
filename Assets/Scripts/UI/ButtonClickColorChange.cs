using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class ButtonClickColorChange : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Color color;
        [SerializeField] private float animationDuration;
        private Color initialColor;

        private void Awake()
        {
            initialColor = button.image.color;
            button.onClick.AddListener(ChangeButtonColor);
        }

        private void ChangeButtonColor()
        {
            Sequence.Create(Tween.Color(button.image, button.image.color, color, animationDuration))
                .Group(Tween.Color(button.image, button.image.color, initialColor, animationDuration));
        }
    }
}