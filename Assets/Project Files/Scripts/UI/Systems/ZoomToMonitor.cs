using System.Collections;
using Kart.Project_Files.Scripts.Managers.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    public class ZoomToMonitor : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        [SerializeField] private Vector2 rawImageXY;
        [SerializeField] private Vector2 rawImageSize;
        [SerializeField] private Button mainButton;
        [SerializeField] private RadialDragRotate radialDragRotate;
        [SerializeField] private float animationTime;
        private Vector2 _originalPosition;
        private Vector2 _originalSize;
        private bool _isZoomed;
        private bool _isZoomInProgress;

        private void Awake()
        {
            _originalPosition = new Vector2(rawImage.uvRect.x, rawImage.uvRect.y);
            _originalSize = new Vector2(rawImage.uvRect.width, rawImage.uvRect.height);
        }

        private void Update()
        {
            if (_isZoomed && InterfaceManager.Instance.ActiveScreen == InterfaceManager.Instance.RootScreen)
            {
                StartCoroutine(ZoomOut());
            }
        }

        public IEnumerator ZoomIn()
        {
            if (_isZoomInProgress || _isZoomed)
                yield break;
            
            mainButton.interactable = false;
            _isZoomInProgress = true;
            rawImage.enabled = true;
            radialDragRotate.isDisabled = true;
            
            float elapsedTime = 0f;

            while (elapsedTime < animationTime)
            {
                rawImage.uvRect = new Rect(
                    Mathf.Lerp(_originalPosition.x, rawImageXY.x, elapsedTime / animationTime),
                    Mathf.Lerp(_originalPosition.y, rawImageXY.y, elapsedTime / animationTime),
                    Mathf.Lerp(_originalSize.x, rawImageSize.x, elapsedTime / animationTime),
                    Mathf.Lerp(_originalSize.y, rawImageSize.y, elapsedTime / animationTime)
                );
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _isZoomInProgress = false;
            _isZoomed = true;
        }

        public IEnumerator ZoomOut()
        {
            if (_isZoomInProgress || !_isZoomed)
                yield break;

            _isZoomed = false;
            _isZoomInProgress = true;
            float elapsedTime = 0f;

            while (elapsedTime < animationTime)
            {
                rawImage.uvRect = new Rect(
                    Mathf.Lerp(rawImageXY.x, _originalPosition.x, elapsedTime / animationTime),
                    Mathf.Lerp(rawImageXY.y, _originalPosition.y, elapsedTime / animationTime),
                    Mathf.Lerp(rawImageSize.x, _originalSize.x, elapsedTime / animationTime),
                    Mathf.Lerp(rawImageSize.y, _originalSize.y, elapsedTime / animationTime)
                );
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            rawImage.enabled = false;
            _isZoomInProgress = false;
            radialDragRotate.isDisabled = false;
            mainButton.interactable = true;
        }
    }
}