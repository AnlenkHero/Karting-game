using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.ResultScene
{
    public class PointsSlider : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private float startFillAmount;
        [SerializeField] private float fillDuration = 1f;
        private Coroutine _fillRoutine;
        
        public void SetPoints(float points, float maxPoints)
        {
            if (fillImage == null)
            {
                Debug.LogError("Fill Image is not assigned in PointsSlider.");
                return;
            }

            float targetFill = Mathf.Clamp(points / maxPoints, 0f, 1f);

            if (_fillRoutine != null)
                StopCoroutine(_fillRoutine);

            _fillRoutine = StartCoroutine(FillRoutine(targetFill, fillDuration));
        }

        private IEnumerator FillRoutine(float targetFill, float duration)
        {
            float startFill = startFillAmount;
            float elapsed   = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fillImage.fillAmount = Mathf.Lerp(startFill, targetFill, elapsed / duration);
                pointsText.text = $"{(int)(fillImage.fillAmount * 100)}%";
                yield return null;
            }

            fillImage.fillAmount = targetFill;
            pointsText.text = $"{(int)(fillImage.fillAmount * 100)}%";
        }
    }
}