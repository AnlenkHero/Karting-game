using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Animations
{
    public class CanvasFader : MonoBehaviour
    {
        public Image group;

        public float fadeInTime = 0f;
        public float fadeOutTime = 1f;
        private bool fading = false;

        public void FadeOut()
        {
            StartCoroutine(FadeRoutine(false));
        }

        public void FadeIn()
        {
            gameObject.SetActive(true);
            StartCoroutine(FadeRoutine(true));
        }

        private IEnumerator FadeRoutine(bool fadeIn)
        {
            float from = fadeIn ? 0 : 1;
            float to = fadeIn ? 1 : 0;
            float fadeFactor = fadeIn ? fadeInTime : fadeOutTime;
            float t = fading ? Mathf.InverseLerp(from, to, group.color.a) : 0;

            fading = true;
            while (t < 1)
            {
                t += Time.deltaTime / fadeFactor;
                group.color = new Color(group.color.r,group.color.g,group.color.b,Mathf.Lerp(from, to, t));
                yield return null;
            }
            group.color = new Color(group.color.r,group.color.g,group.color.b,to);
            fading = false;

            if (!fadeIn) gameObject.SetActive(false);
        }
    }
}