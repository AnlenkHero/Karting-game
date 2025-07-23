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
        [SerializeField] [Range(0f, 1f)] private float startFillAmount;
        [SerializeField] private float fillDuration = 1f;
        private Coroutine _fillRoutine;
        
        private static readonly Color ZeroColor = Color.red;
        private static readonly Color FullColor = Color.green;
        
        public void SetPoints(float points, float maxPoints)
        {
            if (fillImage == null || pointsText == null)
            {
                Debug.LogError("Fill Image or Points Text is not assigned in PointsSlider.");
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
                float t = Mathf.Clamp01(elapsed / duration);
                
                float currentFill = Mathf.Lerp(startFill, targetFill, t);
                fillImage.fillAmount = currentFill;
                
                int percent = Mathf.RoundToInt(currentFill * 100f);
                pointsText.text = percent + "%";
                
                pointsText.color = Color.Lerp(ZeroColor, FullColor, currentFill);
                
                yield return null;
            }
            
            fillImage.fillAmount = targetFill;
            int finalPercent = Mathf.RoundToInt(targetFill * 100f);
            pointsText.text  = finalPercent + "%";
            pointsText.color = Color.Lerp(ZeroColor, FullColor, targetFill);
        }
    }
}
