using System.Collections;
using Kart.Project_Files.Scripts.Managers.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    public class RadialDragRotate : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Transform wheelTransform;
        [SerializeField] private float maxRotation = 1080f;
        
        [Header("Overshoot Damping")]
        [SerializeField] private float overshootDamping = 0.1f;
        [SerializeField] private float pointerOvershootDamping = 1.0f;
        [SerializeField] private float gamepadRotationSpeed = 180f;
        
        [SerializeField] private float autoAnimSpeed = 90f;

        private InterfaceInputHandler _inputHandler;
        private float _angle;
        private float _previousAngle;
        private bool _isBouncingBack;
        private bool _isPointerDragging;
        public bool isAutoAnimating = true;

        private void Awake()
        {
            _inputHandler = InterfaceManager.Instance.inputHandler;
        }

        private void Update()
        {
            if (IntroAutoAnimateSteeringWheel()) return;
            HandleOtherDevicesInput();
        }

        private bool IntroAutoAnimateSteeringWheel()
        {
            if (!isAutoAnimating) return false;
            var horizontalInput = _inputHandler != null ? -_inputHandler.navigationInput.x : 0f;
            if (Mathf.Abs(horizontalInput) > 0.01f)
            {
                isAutoAnimating = false;
            }
            else if (_isPointerDragging)
            {
                isAutoAnimating = false;
            }

            if (!isAutoAnimating) return false;
            _angle = Mathf.PingPong(Time.time * autoAnimSpeed, 180f) - 90f;
            wheelTransform.rotation = Quaternion.Euler(0, 0, _angle);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            return true;

        }

        private void HandleOtherDevicesInput()
        {
            float horizontalInput = _inputHandler != null ? -_inputHandler.navigationInput.x : 0f;
            if (!_isPointerDragging && Mathf.Abs(horizontalInput) > 0.01f)
            {
                if (_isBouncingBack)
                {
                    StopCoroutine(nameof(BounceBackCoroutine));
                    _isBouncingBack = false;
                }

                float deltaAngle = horizontalInput * gamepadRotationSpeed * Time.deltaTime;
                RotateWheelByDelta(deltaAngle, overshootDamping);
            }
            else if (!_isPointerDragging)
            {
                float targetAngle = Mathf.Clamp(_angle, -maxRotation, maxRotation);
                if (Mathf.Abs(_angle - targetAngle) > 0.1f && !_isBouncingBack)
                {
                    StartCoroutine(BounceBackCoroutine(targetAngle));
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (isAutoAnimating)
                isAutoAnimating = false;

            _isPointerDragging = true;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                _previousAngle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isAutoAnimating)
                return; 

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint))
                return;

            float currentAngle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
            float deltaAngle = currentAngle - _previousAngle;

            if (deltaAngle > 180)      deltaAngle -= 360f;
            else if (deltaAngle < -180) deltaAngle += 360f;

            RotateWheelByDelta(deltaAngle, pointerOvershootDamping);
            _previousAngle = currentAngle;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isAutoAnimating)
                return; 

            _isPointerDragging = false;
            float targetAngle = Mathf.Clamp(_angle, -maxRotation, maxRotation);
            if (Mathf.Abs(_angle - targetAngle) > 0.1f)
            {
                if (_isBouncingBack)
                {
                    StopCoroutine(nameof(BounceBackCoroutine));
                }
                StartCoroutine(BounceBackCoroutine(targetAngle));
            }
        }

        private IEnumerator BounceBackCoroutine(float targetAngle)
        {
            _isBouncingBack = true;
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
            _isBouncingBack = false;
        }


        private void RotateWheelByDelta(float deltaAngle, float damping)
        {
            float newAngleCandidate = _angle + deltaAngle;

            if (newAngleCandidate > maxRotation)
            {
                float overshoot = newAngleCandidate - maxRotation;
                deltaAngle *= 1f / (1f + overshoot * damping);
                newAngleCandidate = _angle + deltaAngle;
            }
            else if (newAngleCandidate < -maxRotation)
            {
                float overshoot = -maxRotation - newAngleCandidate;
                deltaAngle *= 1f / (1f + overshoot * damping);
                newAngleCandidate = _angle + deltaAngle;
            }

            _angle = newAngleCandidate;
            wheelTransform.rotation = Quaternion.Euler(0, 0, _angle);
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
    }
}
