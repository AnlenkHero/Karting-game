using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Kart.Project_Files.Scripts.UI.Effects
{
    public class ImageFader : MonoBehaviour
    {
        [SerializeField] private Graphic[] fadeImages;
        [SerializeField] private float fadeDurationDefault = 1f;

        /// <summary>
        /// Fade all images to endValue (0 = fade out, 1 = fade in) simultaneously.
        /// </summary>
        public void PlayFade(float endValue, float animationDuration = 0, Action callback = null)
        {
            float duration = animationDuration <= 0 ? fadeDurationDefault : animationDuration;
            int completedCount = 0;
            int total = fadeImages.Length;

            foreach (var image in fadeImages)
            {
                image.DOFade(endValue, duration).OnComplete(() =>
                {
                    completedCount++;
                    if (completedCount >= total)
                        callback?.Invoke();
                });
            }
        }

        /// <summary>
        /// Fade images sequentially: when one image finishes, the next starts.
        /// </summary>
        public void PlayFadeInQueue(float endValue, float fadeDuration, bool isReverse, Action callback = null)
        {
            var imageArray = fadeImages;
            if (isReverse)
            {
                Array.Reverse(imageArray);
            }
            float duration = fadeDuration <= 0 ? fadeDurationDefault : fadeDuration;
            var sequence = DOTween.Sequence();

            foreach (var image in imageArray)
            {
                sequence.Append(image.DOFade(endValue, duration));
            }

            sequence.OnComplete(() => callback?.Invoke());
        }
    }
}