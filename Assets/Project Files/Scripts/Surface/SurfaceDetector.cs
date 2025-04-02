using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kart.Project_Files.Scripts.Controls;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Surface
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

        public SurfaceType CurrentSurface => currentSurface != null ? currentSurface : defaultSurface;

        private void Awake()
        {
            if (defaultSurface == null)
            {
                Debug.LogError("Default surface is not assigned in the Inspector!", this);
            }
            currentSurface = defaultSurface;
        }

        private void Start()
        {
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

            Debug.Log($"Entered {surfaceArea.surface?.surfaceName ?? "Unknown"} surface");
            overlappingSurfaceAreas.Add(surfaceArea);
            UpdateCurrentSurface();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<SurfaceArea>(out var surfaceArea) ||
                !overlappingSurfaceAreas.Contains(surfaceArea))
                return;

            Debug.Log($"Exited {surfaceArea.surface?.surfaceName ?? "Unknown"} surface");
            overlappingSurfaceAreas.Remove(surfaceArea);
            UpdateCurrentSurface();
        }

        private void UpdateCurrentSurface()
        {
            var newSurfaceArea = overlappingSurfaceAreas
                .OrderBy(sa => sa.priority)
                .LastOrDefault();

            SurfaceType newSurface = defaultSurface;
            if (newSurfaceArea != null)
            {
                if (newSurfaceArea.surface != null)
                {
                    newSurface = newSurfaceArea.surface;
                }
                else
                {
                    Debug.LogWarning("SurfaceArea's surface is null.", newSurfaceArea);
                }
            }
            
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
            if (newSurface == null)
            {
                Debug.LogError("New surface is null. Transition aborted.");
                yield break;
            }

            if (newSurface.smoothTime <= 0f)
            {
                ApplySurfaceModifiersInstant(newSurface);
                yield break;
            }

            float elapsed = 0f;

            float startForwardFriction = oldSurface != null ? oldSurface.forwardFriction : newSurface.forwardFriction;
            float startSidewaysFriction = oldSurface != null ? oldSurface.sidewaysFriction : newSurface.sidewaysFriction;

            while (elapsed < newSurface.smoothTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / newSurface.smoothTime);

                float currentForwardFriction = Mathf.Lerp(startForwardFriction, newSurface.forwardFriction, t);
                float currentSidewaysFriction = Mathf.Lerp(startSidewaysFriction, newSurface.sidewaysFriction, t);
                
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
            if (surface == null)
            {
                Debug.LogError("Surface is null in ApplySurfaceModifiersInstant.");
                return;
            }

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
