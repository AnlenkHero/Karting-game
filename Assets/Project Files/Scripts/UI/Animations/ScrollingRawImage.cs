using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Animations
{
    [RequireComponent(typeof(RawImage))]
    public class ScrollingRawImage : MonoBehaviour
    {
        [SerializeField] private RawImage rawImage;
        public float xSpeed, ySpeed;
        private float _xVal, _yVal;

        private void Update()
        {
            _xVal += Time.deltaTime * xSpeed;
            _yVal += Time.deltaTime * ySpeed;
            rawImage.uvRect = new Rect(_xVal, _yVal, rawImage.uvRect.width, rawImage.uvRect.height);
        }
    }
}