using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kart.UI
{
    public class ButtonScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Vector3 hoveredScale = new Vector3(1.1f, 1.1f, 1.1f);
        [SerializeField] private float animationDuration = 0.2f;
        [SerializeField] private Color buttonDesiredColor;
        [SerializeField] private Color textDesiredColor;
        private Color buttonInitialColor;
        private Color textInitialColor;
        private Tween tween;
        private Sequence sequence;
        private Vector3 originalScale;

        void Awake()
        {
            textInitialColor = text.color;
            buttonInitialColor = image.color;
            originalScale = transform.localScale;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            sequence = Tween.Scale(transform, originalScale, hoveredScale, animationDuration, Ease.InOutCirc)
               // .Group(Tween.Color(image, buttonDesiredColor, animationDuration))
                .Group(Tween.Color(text, textDesiredColor, animationDuration))
                .Chain(Tween.Scale(transform, hoveredScale, originalScale, animationDuration, Ease.InOutSine))
               // .Group(Tween.Color(image, buttonInitialColor, animationDuration))
                .Group(Tween.Color(text, textInitialColor, animationDuration));
            sequence.SetRemainingCycles(-1);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            sequence.Stop();
            sequence = Tween.Scale(transform, transform.localScale, originalScale, animationDuration)
                //.Group(Tween.Color(image, buttonInitialColor, animationDuration))
                .Group(Tween.Color(text, textInitialColor, animationDuration));
        }
    }
}