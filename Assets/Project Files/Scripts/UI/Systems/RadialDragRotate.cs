using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    public class RadialDragRotate : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RadialLayout radialLayout;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Transform wheelTransform;
        [SerializeField] private float maxRotation = 1080f;
        [SerializeField] private float overshootDamping = 0.1f; 
        private float _angle;
        private float _previousAngle;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                _previousAngle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
                return;

            float currentAngle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
            float deltaAngle = currentAngle - _previousAngle;
            
            switch (deltaAngle)
            {
                case > 180:
                    deltaAngle -= 360;
                    break;
                case < -180:
                    deltaAngle += 360;
                    break;
            }
            
            float newAngleCandidate = _angle + deltaAngle;
            
            if (newAngleCandidate > maxRotation)
            {
                float overshoot = newAngleCandidate - maxRotation;
                deltaAngle *= 1f / (1f + overshoot * overshootDamping);
                newAngleCandidate = _angle + deltaAngle;
            }
            else if (newAngleCandidate < -maxRotation)
            {
                float overshoot = -maxRotation - newAngleCandidate;
                deltaAngle *= 1f / (1f + overshoot * overshootDamping);
                newAngleCandidate = _angle + deltaAngle;
            }
            
            _angle = newAngleCandidate;
            wheelTransform.rotation = Quaternion.Euler(0, 0, _angle);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

            _previousAngle = currentAngle;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            float targetAngle = Mathf.Clamp(_angle, -maxRotation, maxRotation);
            if (Mathf.Abs(_angle - targetAngle) > 0.1f)
            {
                StopCoroutine(nameof(BounceBackCoroutine));
                StartCoroutine(BounceBackCoroutine(targetAngle));
            }
        }

        private IEnumerator BounceBackCoroutine(float targetAngle)
        {
            float duration = 0.2f;
            float startAngle = _angle;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float newAngle = Mathf.Lerp(startAngle, targetAngle, t);
                _angle = newAngle;
                wheelTransform.rotation = Quaternion.Euler(0, 0, newAngle);
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
                yield return null;
            }

            _angle = targetAngle;
            wheelTransform.rotation = Quaternion.Euler(0, 0, targetAngle);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }
}
