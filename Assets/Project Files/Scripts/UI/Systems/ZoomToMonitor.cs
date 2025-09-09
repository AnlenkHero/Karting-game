using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Kart.Project_Files.Scripts.Managers.Interface;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    public class ZoomToMonitor : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private Vector2 rawImageXY;
        [SerializeField] private Vector2 rawImageSize;
        [SerializeField] private Button mainButton;
        [SerializeField] private RadialDragRotate radialDragRotate;
        [SerializeField] private float animationTime = 0.5f;

        private Vector2 _originalPosition;
        private Vector2 _originalSize;
        private bool _isZoomed;
        private bool _isZoomInProgress;

        private void Awake()
        {
            _originalPosition = new Vector2(rawImage.uvRect.x, rawImage.uvRect.y);
            _originalSize     = new Vector2(rawImage.uvRect.width, rawImage.uvRect.height);
            rawImage.enabled  = false;
        }

        private void Update()
        {
            if (_isZoomed && InterfaceManager.Instance.ActiveScreen == InterfaceManager.Instance.RootScreen)
                StartCoroutine(ZoomOut());
        }

        public IEnumerator ZoomIn()
        {
            if (_isZoomInProgress || _isZoomed) yield break;
            _isZoomInProgress = true;

            mainButton.interactable   = false;
            radialDragRotate.isDisabled = true;
            rawImage.enabled          = true;

            Tween tween = DOTween.To(() => rawImage.uvRect, r => rawImage.uvRect = r,
                new Rect(rawImageXY.x, rawImageXY.y, rawImageSize.x, rawImageSize.y),
                animationTime)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    _isZoomed         = true;
                    _isZoomInProgress = false;
                });
            yield return tween.WaitForCompletion();
        }

        public IEnumerator ZoomOut()
        {
            if (_isZoomInProgress || !_isZoomed) yield break;
            _isZoomInProgress = true;
            _isZoomed         = false;

            Tween tween = DOTween.To(() => rawImage.uvRect, r => rawImage.uvRect = r,
                new Rect(_originalPosition.x, _originalPosition.y, _originalSize.x, _originalSize.y),
                animationTime)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    rawImage.enabled       = false;
                    radialDragRotate.isDisabled = false;
                    mainButton.interactable = true;
                    _isZoomInProgress       = false;
                });
            yield return tween.WaitForCompletion();
        }
    }
}
