using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.UI.Extensions
{
    public class RadialDragRotate : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RadialLayout radialLayout;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Transform wheelTransform;
        [SerializeField] private float maxRotation = 1080f;
        [SerializeField] private float overshootDamping = 0.1f; 
        
        private float previousAngle;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                previousAngle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
                return;

            float currentAngle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
            float deltaAngle = currentAngle - previousAngle;
            
            switch (deltaAngle)
            {
                case > 180:
                    deltaAngle -= 360;
                    break;
                case < -180:
                    deltaAngle += 360;
                    break;
            }
            
            float newAngleCandidate = radialLayout.StartAngle + deltaAngle;
            
            if (newAngleCandidate > maxRotation)
            {
                float overshoot = newAngleCandidate - maxRotation;
                deltaAngle *= 1f / (1f + overshoot * overshootDamping);
                newAngleCandidate = radialLayout.StartAngle + deltaAngle;
            }
            else if (newAngleCandidate < -maxRotation)
            {
                float overshoot = -maxRotation - newAngleCandidate;
                deltaAngle *= 1f / (1f + overshoot * overshootDamping);
                newAngleCandidate = radialLayout.StartAngle + deltaAngle;
            }
            
            radialLayout.StartAngle = newAngleCandidate;
            wheelTransform.rotation = Quaternion.Euler(0, 0, radialLayout.StartAngle);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

            previousAngle = currentAngle;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            float targetAngle = Mathf.Clamp(radialLayout.StartAngle, -maxRotation, maxRotation);
            if (Mathf.Abs(radialLayout.StartAngle - targetAngle) > 0.1f)
            {
                StopCoroutine(nameof(BounceBackCoroutine));
                StartCoroutine(BounceBackCoroutine(targetAngle));
            }
        }

        private IEnumerator BounceBackCoroutine(float targetAngle)
        {
            float duration = 0.2f;
            float startAngle = radialLayout.StartAngle;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float newAngle = Mathf.Lerp(startAngle, targetAngle, t);
                radialLayout.StartAngle = newAngle;
                wheelTransform.rotation = Quaternion.Euler(0, 0, newAngle);
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
                yield return null;
            }

            radialLayout.StartAngle = targetAngle;
            wheelTransform.rotation = Quaternion.Euler(0, 0, targetAngle);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }
}
