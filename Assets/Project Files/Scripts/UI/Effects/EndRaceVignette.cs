using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Kart.Project_Files.Scripts.UI.Effects
{
    public class EndRaceVignette : MonoBehaviour
    {
        [Tooltip("Drag your VignetteOverlay Image here")]
        [SerializeField] private Image vignetteImage;
        [Tooltip("How long the fade takes (seconds)")]
        [SerializeField] private float fadeDuration = 1.5f;

        private void Awake()
        {
            var c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
        }
        
        public void PlayVignetteFadeIn(Action callback = null)
        {
            vignetteImage.DOFade(1f, fadeDuration)
                .OnComplete(() =>
                {
                    callback?.Invoke();
                });
        }
    }
}