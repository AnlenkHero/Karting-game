using Kart.Managers;
using UnityEngine;

namespace Kart.Controls
{
    public class KartAudio : MonoBehaviour
    {
        public AudioSource StartSound;
        public AudioSource IdleSound;
        public AudioSource RunningSound;
        public AudioSource ReverseSound;
        public AudioSource Drift;
        public AudioSource Boost;
        public AudioSource Offroad;
        public AudioSource Crash;
        public AudioSource Horn;
        [Range(0.1f, 1.0f)] public float RunningSoundMaxVolume = 1.0f;
        [Range(0.1f, 2.0f)] public float RunningSoundMaxPitch = 1.0f;
        [Range(0.1f, 1.0f)] public float ReverseSoundMaxVolume = 0.5f;
        [Range(0.1f, 2.0f)] public float ReverseSoundMaxPitch = 0.6f;
        [Range(0.1f, 1.0f)] public float IdleSoundMaxVolume = 0.6f;

        [Range(0.1f, 1.0f)] public float DriftMaxVolume = 0.5f;

        [SerializeField] private KartController kartController;

        /*   public  void Awake()
           {
               kartController.OnSpinoutChanged += val =>
               {
                   if (!val) return;
                   AudioManager.PlayAndFollow("slipSFX", transform, AudioManager.MixerTarget.SFX);
               };

           }*/

        public void Update()
        {
            float speed = kartController.SignedVelocityMagnitude;
            HandleDriftAudio(speed);
            // HandleOffroadAudio(kartController.Velocity.magnitude);
            HandleDriveAudio(speed);

            IdleSound.volume = Mathf.Lerp(IdleSoundMaxVolume, 0.0f, kartController.Velocity.magnitude);
        }

        private void HandleDriveAudio(float speed)
        {
            float forwardVolume = 0.0f;
            float reverseVolume = 0.0f;

            if (speed < 0.0f)
            {
                ApplyDriveAudio(ReverseSound, 0.1f, 0.1f, ReverseSoundMaxVolume, ReverseSoundMaxPitch, speed, kartController.MaxReverseSpeed, out reverseVolume);
            }
            else
            {
                ApplyDriveAudio(RunningSound, 0.1f, 0.3f, RunningSoundMaxVolume, RunningSoundMaxPitch, speed, kartController.MaxSpeed, out forwardVolume);
            }
            
            RunningSound.volume = Mathf.Lerp(RunningSound.volume, forwardVolume, Time.deltaTime * 5f);
            ReverseSound.volume = Mathf.Lerp(ReverseSound.volume, reverseVolume, Time.deltaTime * 5f);
        }


        private void ApplyDriveAudio(AudioSource audioSource, float minVolume, float minPitch, float maxVolume,
            float maxPitch, float speed, float maxSpeed, out float targetVolume)
        {
            targetVolume = Mathf.Lerp(minVolume, maxVolume, Mathf.Abs(speed / maxSpeed));
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(speed / maxSpeed + (Mathf.Sin(Time.time) * 0.1f)));
        }


        private void HandleDriftAudio(float speed)
        {
            var b = kartController.IsDrifting() && kartController.IsGrounded()
                ? speed / kartController.MaxSpeed * DriftMaxVolume
                : 0.0f;
            Drift.volume = Mathf.Lerp(Drift.volume, b, Time.deltaTime * 20f);
        }

        /*   private void PlaySurfaceAudio()
           {
               Offroad.volume = kartController.IsOffroad
                   ? Mathf.Lerp(0, 0.25f, Mathf.Abs(speed) * 1.2f)
                   : Mathf.Lerp(Offroad.volume, 0, Time.deltaTime * 10f);
           }*/

        public void PlayHorn()
        {
            Horn.Play();
        }
    }
}