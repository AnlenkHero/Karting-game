using PrimeTween;
using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Animations.Button
{
    public class ButtonClickColorChange : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
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