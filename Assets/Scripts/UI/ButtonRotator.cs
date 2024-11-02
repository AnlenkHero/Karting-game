using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kart.UI
{
    public class ButtonRotator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Vector3 hoveredRotation = new Vector3(0f, 0, 180f);
        [SerializeField] private float animationDuration = 0.5f;
        
        private Tween tween;
        private Sequence sequence;
        private Vector3 originalRotation;

        void Awake()
        {
            originalRotation = transform.localEulerAngles;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            sequence = Tween.LocalEulerAngles(transform, originalRotation, hoveredRotation, animationDuration, Ease.InOutCirc)
                .Chain(Tween.LocalEulerAngles(transform, hoveredRotation, originalRotation, animationDuration, Ease.InOutSine));
            sequence.SetRemainingCycles(-1);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            sequence.Stop();
            tween = Tween.LocalEulerAngles(transform, transform.localEulerAngles, originalRotation, animationDuration);

        }
    }
}