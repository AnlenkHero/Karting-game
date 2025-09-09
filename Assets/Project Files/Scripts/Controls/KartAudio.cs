using System.Collections;
using Kart.Project_Files.Scripts.Surface;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Controls
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
        [SerializeField] private float audioFadeLerpSpeed = 5f;
        
        [Header("Surface crossFade settings")] [SerializeField]
        private float crossFadeDuration = 0.5f;

        private AudioSource _activeSurfaceSource;
        private AudioSource _inactiveSurfaceSource;
        private Coroutine _crossFadeRoutine;

        [Header("References")] [SerializeField]
        private SkidMarkHandler skidMarkHandler;

        [SerializeField] private KartController kartController;

        [Header("Drift Volume Settings")] [SerializeField, Range(0f, 1f)]
        private float driftMinVolume = 0.1f;

        [SerializeField, Range(0.01f, 1f)] private float driftVolumeSmoothTime = 0.2f;
        [SerializeField] private float driftFadeSpeed = 20f;
        [SerializeField] private AnimationCurve driftCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private int _activeSkids;
        private float _driftVolumeVelocity;

        private void Awake()
        {
            InitializeCrossFade();
            skidMarkHandler.SkidStarted += OnWheelSkidStarted;
            skidMarkHandler.SkidEnded += OnWheelSkidEnded;
        }

        private void OnDestroy()
        {
            skidMarkHandler.SkidStarted -= OnWheelSkidStarted;
            skidMarkHandler.SkidEnded -= OnWheelSkidEnded;
        }

        public void Update()
        {
            HandleDriftAudio();
            HandleDriveAudio(kartController.NetworkedSignedVelocityMagnitude);

            idleSound.volume = Mathf.Lerp(idleSoundMaxVolume, 0.0f, kartController.NetworkedVelocity.magnitude);
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

            runningSound.volume = Mathf.Lerp(runningSound.volume, forwardVolume, Time.deltaTime * audioFadeLerpSpeed);
            reverseSound.volume = Mathf.Lerp(reverseSound.volume, reverseVolume, Time.deltaTime * audioFadeLerpSpeed);
        }


        private void ApplyDriveAudio(AudioSource audioSource, float minVolume, float minPitch, float maxVolume,
            float maxPitch, float speed, float maxSpeed, out float targetVolume)
        {
            targetVolume = Mathf.Lerp(minVolume, maxVolume, Mathf.Abs(speed / maxSpeed));
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch,
                Mathf.Abs(speed / maxSpeed + (Mathf.Sin(Time.time) * 0.1f)));
        }

        #region Drift Audio

        private void HandleDriftAudio()
        {
            float targetVol = 0f;
            if (_activeSkids > 0)
            {
                Vector3 localVel = kartController.NetworkedVelocity;
                float lateral = Mathf.Abs(localVel.x);
                float slipNorm = Mathf.Clamp01(lateral / kartController.MaxSpeed);
                float curveVal = driftCurve.Evaluate(slipNorm);
                targetVol = Mathf.Lerp(driftMinVolume, driftMaxVolume, curveVal);
            }

            drift.volume = Mathf.SmoothDamp(drift.volume, targetVol, ref _driftVolumeVelocity, driftVolumeSmoothTime);

            if (_activeSkids != 0 || !drift.isPlaying || !(drift.volume < 0.01f)) return;

            drift.Stop();
            drift.loop = false;
            _driftVolumeVelocity = 0f;
        }

        private void OnWheelSkidStarted()
        {
            _activeSkids++;
            if (_activeSkids != 1) return;
            drift.volume = 0f;
            drift.loop = true;
            drift.Play();
        }

        private void OnWheelSkidEnded()
        {
            _activeSkids = Mathf.Max(0, _activeSkids - 1);
        }

        #endregion

        #region Surface CrossFade

        public void PlaySurfaceAudioCrossFade(SurfaceType surface)
        {
            if (surface.audioClip == null)
            {
                FadeOutAndStopActiveSource();
                return;
            }

            if (_activeSurfaceSource.clip == surface.audioClip && _activeSurfaceSource.isPlaying)
                return;

            if (_crossFadeRoutine != null)
            {
                StopCoroutine(_crossFadeRoutine);
                _crossFadeRoutine = null;
            }

            _inactiveSurfaceSource.clip = surface.audioClip;
            _inactiveSurfaceSource.loop = surface.isContinuousEffect;
            _inactiveSurfaceSource.volume = 0f;
            _inactiveSurfaceSource.Play();

            _crossFadeRoutine = StartCoroutine(CrossFadeRoutine(surface.isContinuousEffect));
        }

        private void FadeOutAndStopActiveSource()
        {
            if (_crossFadeRoutine != null)
            {
                StopCoroutine(_crossFadeRoutine);
                _crossFadeRoutine = null;
            }

            _crossFadeRoutine = StartCoroutine(FadeOutActiveSourceRoutine());
        }

        private IEnumerator FadeOutActiveSourceRoutine()
        {
            float startVolume = _activeSurfaceSource.volume;
            float t = 0f;

            while (t < crossFadeDuration)
            {
                t += Time.deltaTime;
                float factor = Mathf.Clamp01(t / crossFadeDuration);
                _activeSurfaceSource.volume = Mathf.Lerp(startVolume, 0f, factor);
                yield return null;
            }

            _activeSurfaceSource.volume = 0f;
            _activeSurfaceSource.Stop();
            _activeSurfaceSource.clip = null;
            _crossFadeRoutine = null;
        }

        private IEnumerator CrossFadeRoutine(bool isContinuous)
        {
            float startVolumeActive = _activeSurfaceSource.volume;
            float startVolumeInactive = _inactiveSurfaceSource.volume;
            float endVolumeInactive = 1f;

            float t = 0f;
            while (t < crossFadeDuration)
            {
                t += Time.deltaTime;
                float factor = Mathf.Clamp01(t / crossFadeDuration);

                _activeSurfaceSource.volume = Mathf.Lerp(startVolumeActive, 0f, factor);
                _inactiveSurfaceSource.volume = Mathf.Lerp(startVolumeInactive, endVolumeInactive, factor);

                yield return null;
            }

            _activeSurfaceSource.volume = 0f;
            _activeSurfaceSource.Stop();
            _activeSurfaceSource.clip = null;

            _inactiveSurfaceSource.volume = endVolumeInactive;

            (_activeSurfaceSource, _inactiveSurfaceSource) = (_inactiveSurfaceSource, _activeSurfaceSource);

            _crossFadeRoutine = null;
        }

        private void InitializeCrossFade()
        {
            _activeSurfaceSource = surfaceA;
            _inactiveSurfaceSource = surfaceB;
            surfaceA.Stop();
            surfaceB.Stop();
            surfaceA.volume = 0f;
            surfaceB.volume = 0f;
        }

        #endregion

        public void PlayHorn()
        {
            horn.Play();
        }
    }
}