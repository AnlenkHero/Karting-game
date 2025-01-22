using System.Collections;
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
        [SerializeField] private KartAudio kartAudio;

        [Header("Surface Settings")]
        [SerializeField] private SurfaceType defaultSurface;

        private List<SurfaceArea> overlappingSurfaceAreas = new();
        private SurfaceType currentSurface;

        private Coroutine transitionRoutine;
        private bool isContinuousEffect;

        public SurfaceType CurrentSurface => currentSurface ?? defaultSurface;

        private void Start()
        {
            currentSurface = defaultSurface;
            ApplySurfaceModifiersInstant(currentSurface);
        }

        private void FixedUpdate()
        {
            if (isContinuousEffect)
            {
                currentSurface?.customBehavior?.ApplyBehavior(kartController, currentSurface);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<SurfaceArea>(out var surfaceArea) ||
                overlappingSurfaceAreas.Contains(surfaceArea))
                return;

            Debug.Log($"Entered {surfaceArea.surface.surfaceName} surface");
            overlappingSurfaceAreas.Add(surfaceArea);
            UpdateCurrentSurface();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<SurfaceArea>(out var surfaceArea) ||
                !overlappingSurfaceAreas.Contains(surfaceArea))
                return;

            Debug.Log($"Exited {surfaceArea.surface.surfaceName} surface");
            overlappingSurfaceAreas.Remove(surfaceArea);
            UpdateCurrentSurface();
        }

        private void UpdateCurrentSurface()
        {
            var newSurfaceArea = overlappingSurfaceAreas
                .OrderBy(sa => sa.priority) 
                .LastOrDefault();

            var newSurface = (newSurfaceArea != null)
                ? newSurfaceArea.surface
                : defaultSurface;

            if (newSurface == currentSurface) return;

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }

            transitionRoutine = StartCoroutine(SmoothTransitionRoutine(currentSurface, newSurface));

            isContinuousEffect = newSurface.isContinuousEffect && newSurface.customBehavior != null;
            currentSurface = newSurface;

            ApplyOneShotSurfaceBehaviour();

            if (kartAudio != null)
            {
                kartAudio.PlaySurfaceAudioCrossFade(currentSurface);
            }
        }

        private IEnumerator SmoothTransitionRoutine(SurfaceType oldSurface, SurfaceType newSurface)
        {
            if (newSurface.smoothTime <= 0f)
            {
                ApplySurfaceModifiersInstant(newSurface);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < newSurface.smoothTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / newSurface.smoothTime);

                float currentForwardFriction = Mathf.Lerp(oldSurface.forwardFriction, newSurface.forwardFriction, t);
                float currentSidewaysFriction = Mathf.Lerp(oldSurface.sidewaysFriction, newSurface.sidewaysFriction, t);

                kartController.slowdownMultiplier = Mathf.Lerp(kartController.slowdownMultiplier, newSurface.slowdownMultiplier, t);
                kartController.frictionMultiplier = Mathf.Lerp(kartController.frictionMultiplier, newSurface.frictionMultiplier, t);
                kartController.steeringSensitivityMultiplier = Mathf.Lerp(kartController.steeringSensitivityMultiplier, newSurface.steeringSensitivityMultiplier, t);
                kartController.brakeMultiplier = Mathf.Lerp(kartController.brakeMultiplier, newSurface.brakeMultiplier, t);

                kartController.SetSurfaceFriction(currentForwardFriction, currentSidewaysFriction);

                yield return null;
            }

            ApplySurfaceModifiersInstant(newSurface);
            transitionRoutine = null;
        }

        private void ApplySurfaceModifiersInstant(SurfaceType surface)
        {
            kartController.slowdownMultiplier = surface.slowdownMultiplier;
            kartController.frictionMultiplier = surface.frictionMultiplier;
            kartController.steeringSensitivityMultiplier = surface.steeringSensitivityMultiplier;
            kartController.brakeMultiplier = surface.brakeMultiplier;
            kartController.SetSurfaceFriction(surface.forwardFriction, surface.sidewaysFriction);
        }

        private void ApplyOneShotSurfaceBehaviour()
        {
            if (isContinuousEffect) return;
            currentSurface?.customBehavior?.ApplyBehavior(kartController, currentSurface);
        }
    }
}
