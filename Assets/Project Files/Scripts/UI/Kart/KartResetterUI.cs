using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Kart
{
    public class KartResetterUI : MonoBehaviour
    {
        [Header("Refs")] 
        [SerializeField] private Image resetIcon;
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private GameObject vignettePanel;
        [SerializeField] private TextMeshProUGUI resetText;

        void Awake()
        {
            if (resetIcon)
            {
                resetIcon.fillAmount = 1f;
                resetIcon.gameObject.SetActive(false);
            }

            if (resetText) resetText.gameObject.SetActive(false);
        }


        public void ShowHint(string text)
        {
            if (!resetText || !vignettePanel)
            {
                Debug.LogWarning("Reset Text or Vignette Panel is not assigned in KartResetterUI.");
                return;
            }
            
            resetText.text = text;
            
            vignettePanel.SetActive(true);
            resetText.gameObject.SetActive(true);
        }

        public void HideHint()
        {
            if (!resetText || !vignettePanel)
            {
                Debug.LogWarning("Reset Text or Vignette Panel is not assigned in KartResetterUI.");
                return;
            }
            
            resetText.gameObject.SetActive(false);
            vignettePanel.SetActive(false);
        }


        public void ShowCountdown()
        {
            if (!resetIcon || !countdownText || !vignettePanel)
            {
                Debug.LogWarning("Reset Icon or Countdown Text or Vignette Panel is not assigned in KartResetterUI.");
                return;
            }
            resetIcon.fillAmount = 1f;
            
            vignettePanel.SetActive(true);
            resetIcon.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(true);
        }

        public void UpdateCountdown(float remainingSeconds, float totalSeconds)
        {
            if (!resetIcon) return;
            if (totalSeconds <= 0f) totalSeconds = 0.001f;
            float t = Mathf.Clamp01(remainingSeconds / totalSeconds);
            
            countdownText.text = Mathf.RoundToInt(remainingSeconds).ToString();
            resetIcon.fillAmount = t;
        }

        public void HideCountdown()
        {
            if (!resetIcon || !countdownText)
            {
                Debug.LogWarning("Reset Icon or Countdown Text or Vignette Panel is not assigned in KartResetterUI.");
                return;
            }
            
            resetIcon.fillAmount = 1f;
            
            resetIcon.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(false);
            vignettePanel.SetActive(false);
        }
    }
}