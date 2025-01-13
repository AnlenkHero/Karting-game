using System;
using Kart.Controls;
using UnityEngine;

namespace Kart.Surface
{
    public class SurfaceDetector : MonoBehaviour
    {
        [SerializeField] private KartController kartController;
        [SerializeField] private SurfaceType defaultSurface;
        private SurfaceType currentSurface;
        private bool isContinuousEffect;
        public SurfaceType CurrentSurface => currentSurface ?? defaultSurface;

        private void FixedUpdate()
        {
            ApplySurfaceModifiers();
            
            if (isContinuousEffect)
            {
                ApplySurfaceBehavior();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out SurfaceArea surfaceArea)) return;

            currentSurface = surfaceArea.surface;
            isContinuousEffect = currentSurface.customBehavior.IsContinuous;
            
            Debug.Log($"Entered surface: {currentSurface.surfaceName}");
            ApplySurfaceBehavior();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out SurfaceArea surfaceArea) || surfaceArea.surface != currentSurface) return;
            
            isContinuousEffect = false;
            currentSurface = defaultSurface;
            Debug.Log($"Exited surface: {surfaceArea.surface.surfaceName}");
        }

        public void ApplySurfaceBehavior()
        {
            currentSurface?.customBehavior?.ApplyBehavior(kartController, currentSurface);
        }

        private void ApplySurfaceModifiers()
        {
            kartController.slowdownMultiplier = currentSurface.slowdownMultiplier;
            kartController.frictionMultiplier = currentSurface.frictionMultiplier;
            kartController.steeringSensitivityMultiplier = currentSurface.steeringSensitivityMultiplier;
        }
    }
}