using System.Collections;
using Kart.Surface;
using UnityEngine;

namespace Kart.Controls
{
    public class KartAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource startSound;
        [SerializeField] private AudioSource idleSound;
        [SerializeField] private AudioSource runningSound;
        [SerializeField] private AudioSource reverseSound;
        [SerializeField] private AudioSource drift;
        [SerializeField] private AudioSource boost;
        [SerializeField] private AudioSource surfaceA;
        [SerializeField] private AudioSource surfaceB;
        [SerializeField] private AudioSource crash;
        [SerializeField] private AudioSource horn;

        [SerializeField] [Range(0.1f, 1.0f)] private float runningSoundMaxVolume = 1.0f;
        [SerializeField] [Range(0.1f, 2.0f)] private float runningSoundMaxPitch = 1.0f;
        [SerializeField] [Range(0.1f, 1.0f)] private float reverseSoundMaxVolume = 0.5f;
        [SerializeField] [Range(0.1f, 2.0f)] private float reverseSoundMaxPitch = 0.6f;
        [SerializeField] [Range(0.1f, 1.0f)] private float idleSoundMaxVolume = 0.6f;
        [SerializeField] [Range(0.1f, 1.0f)] private float driftMaxVolume = 0.5f;

        [Header("Surface crossFade settings")] [SerializeField]
        private float crossFadeDuration = 0.5f;

        private AudioSource activeSurfaceSource;
        private AudioSource inactiveSurfaceSource;
        private Coroutine crossFadeRoutine;

        [SerializeField] private KartController kartController;

        private void Awake()
        {
            InitializeCrossFade();
        }

        public void Update()
        {
            HandleDriftAudio(kartController.SignedVelocityMagnitude);
            HandleDriveAudio(kartController.SignedVelocityMagnitude);

            idleSound.volume = Mathf.Lerp(idleSoundMaxVolume, 0.0f, kartController.Velocity.magnitude);
        }

        private void HandleDriveAudio(float speed)
        {
            float forwardVolume = 0.0f;
            float reverseVolume = 0.0f;

            if (speed < 0.0f)
            {
                ApplyDriveAudio(reverseSound, 0.1f, 0.1f, reverseSoundMaxVolume, reverseSoundMaxPitch, speed,
                    kartController.MaxReverseSpeed, out reverseVolume);
            }
            else
            {
                ApplyDriveAudio(runningSound, 0.1f, 0.3f, runningSoundMaxVolume, runningSoundMaxPitch, speed,
                    kartController.MaxSpeed, out forwardVolume);
            }

            runningSound.volume = Mathf.Lerp(runningSound.volume, forwardVolume, Time.deltaTime * 5f);
            reverseSound.volume = Mathf.Lerp(reverseSound.volume, reverseVolume, Time.deltaTime * 5f);
        }


        private void ApplyDriveAudio(AudioSource audioSource, float minVolume, float minPitch, float maxVolume,
            float maxPitch, float speed, float maxSpeed, out float targetVolume)
        {
            targetVolume = Mathf.Lerp(minVolume, maxVolume, Mathf.Abs(speed / maxSpeed));
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch,
                Mathf.Abs(speed / maxSpeed + (Mathf.Sin(Time.time) * 0.1f)));
        }


        private void HandleDriftAudio(float speed)
        {
            var b = kartController.IsDrifting() && kartController.IsGrounded()
                ? speed / kartController.MaxSpeed * driftMaxVolume
                : 0.0f;
            drift.volume = Mathf.Lerp(drift.volume, b, Time.deltaTime * 20f);
        }
        
        public void PlaySurfaceAudioCrossFade(SurfaceType surface)
        {
            if (surface.audioClip == null)
            {
                FadeOutAndStopActiveSource();
                return;
            }
            
            if (activeSurfaceSource.clip == surface.audioClip && activeSurfaceSource.isPlaying)
                return;

            if (crossFadeRoutine != null)
            {
                StopCoroutine(crossFadeRoutine);
                crossFadeRoutine = null;
            }
            
            inactiveSurfaceSource.clip = surface.audioClip;
            inactiveSurfaceSource.loop = surface.isContinuousEffect;
            inactiveSurfaceSource.volume = 0f; 
            inactiveSurfaceSource.Play();
            
            crossFadeRoutine = StartCoroutine(CrossFadeRoutine(surface.isContinuousEffect));
        }
        
        private void FadeOutAndStopActiveSource()
        {
            if (crossFadeRoutine != null)
            {
                StopCoroutine(crossFadeRoutine);
                crossFadeRoutine = null;
            }

            crossFadeRoutine = StartCoroutine(FadeOutActiveSourceRoutine());
        }

        private IEnumerator FadeOutActiveSourceRoutine()
        {
            float startVolume = activeSurfaceSource.volume;
            float t = 0f;

            while (t < crossFadeDuration)
            {
                t += Time.deltaTime;
                float factor = Mathf.Clamp01(t / crossFadeDuration);
                activeSurfaceSource.volume = Mathf.Lerp(startVolume, 0f, factor);
                yield return null;
            }

            activeSurfaceSource.volume = 0f;
            activeSurfaceSource.Stop();
            activeSurfaceSource.clip = null;
            crossFadeRoutine = null;
        }
        
        private IEnumerator CrossFadeRoutine(bool isContinuous)
        {
            float startVolumeActive = activeSurfaceSource.volume;
            float startVolumeInactive = inactiveSurfaceSource.volume;
            float endVolumeInactive = 1f;

            float t = 0f;
            while (t < crossFadeDuration)
            {
                t += Time.deltaTime;
                float factor = Mathf.Clamp01(t / crossFadeDuration);
                
                activeSurfaceSource.volume = Mathf.Lerp(startVolumeActive, 0f, factor);
                inactiveSurfaceSource.volume = Mathf.Lerp(startVolumeInactive, endVolumeInactive, factor);

                yield return null;
            }
            
            activeSurfaceSource.volume = 0f;
            activeSurfaceSource.Stop();
            activeSurfaceSource.clip = null;

            inactiveSurfaceSource.volume = endVolumeInactive;
            
            (activeSurfaceSource, inactiveSurfaceSource) = (inactiveSurfaceSource, activeSurfaceSource);

            crossFadeRoutine = null;
        }

        private void InitializeCrossFade()
        {
            activeSurfaceSource = surfaceA;
            inactiveSurfaceSource = surfaceB;
            surfaceA.Stop();
            surfaceB.Stop();
            surfaceA.volume = 0f;
            surfaceB.volume = 0f;
        }

        public void PlayHorn()
        {
            horn.Play();
        }
    }
}