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
            ApplySurfaceModifiers();
            ApplySurfaceBehavior();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out SurfaceArea surfaceArea) || surfaceArea.surface != currentSurface) return;
            
            isContinuousEffect = false;
            currentSurface = defaultSurface;
            Debug.Log($"Exited surface: {surfaceArea.surface.surfaceName}");
            ApplySurfaceModifiers();
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
            kartController.brakeMultiplier = currentSurface.brakeMultiplier;
            kartController.SetSurfaceFriction(currentSurface.forwardFriction, currentSurface.sidewaysFriction);
        }
    }
}