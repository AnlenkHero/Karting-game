using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kart.Controls;

namespace Kart.Surface
{
    public class SurfaceDetector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private KartController kartController;
        
        [Header("Surface Settings")]
        [SerializeField] private SurfaceType defaultSurface;

        // Keep track of ALL SurfaceArea objects currently overlapping
        private List<SurfaceArea> overlappingSurfaceAreas = new List<SurfaceArea>();

        // Which surface are we currently using?
        private SurfaceType currentSurface;
        private bool isContinuousEffect;

        public SurfaceType CurrentSurface => currentSurface ?? defaultSurface;

        private void Start()
        {
            // Start with the default surface
            currentSurface = defaultSurface;
        }

        private void FixedUpdate()
        {
            if (isContinuousEffect)
            {
                ApplySurfaceBehavior();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<SurfaceArea>(out var surfaceArea) || overlappingSurfaceAreas.Contains(surfaceArea)) return;
            
            Debug.Log($"{name} (ScriptName) triggered by {other.name}", this);
            overlappingSurfaceAreas.Add(surfaceArea);
            UpdateCurrentSurface();
        }

        private void OnTriggerExit(Collider other)
        {
            // If we exit a known SurfaceArea, remove it from the list
            if (other.TryGetComponent<SurfaceArea>(out var surfaceArea))
            {
                if (overlappingSurfaceAreas.Contains(surfaceArea))
                {
                    overlappingSurfaceAreas.Remove(surfaceArea);
                }
                UpdateCurrentSurface();
            }
        }

        /// <summary>
        /// Recomputes which surface has the highest priority and applies it.
        /// </summary>
        private void UpdateCurrentSurface()
        {
            // Pick the SurfaceArea with the highest priority
            var highestPrioritySurfaceArea = overlappingSurfaceAreas
                .OrderByDescending(sa => sa.priority)
                .FirstOrDefault();

            // Use the found surface, or default if none
            var newSurface = (highestPrioritySurfaceArea != null)
                ? highestPrioritySurfaceArea.surface
                : defaultSurface;

            // If the surface changed, update everything
            if (newSurface != currentSurface)
            {
                currentSurface = newSurface;
                ApplySurfaceModifiers();
                ApplySurfaceBehavior();

                // Decide if we apply this surface behavior continuously in FixedUpdate
                isContinuousEffect = currentSurface.customBehavior != null 
                                     && currentSurface.isContinuousEffect;

                Debug.Log($"Switched to surface: {currentSurface.surfaceName}");
            }
        }

        /// <summary>
        /// Apply the custom behavior of the current surface (if any).
        /// </summary>
        private void ApplySurfaceBehavior()
        {
            currentSurface?.customBehavior?.ApplyBehavior(kartController, currentSurface);
        }

        /// <summary>
        /// Update all of the kart's multipliers based on the current surface.
        /// </summary>
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
