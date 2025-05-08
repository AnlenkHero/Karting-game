using System;
using Fusion;
using Kart.Project_Files.Scripts.Managers;
using Kart.Project_Files.Scripts.Surface;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kart.Project_Files.Scripts.Controls
{
    public class KartAnimator : NetworkBehaviour
    {
        [SerializeField] private ParticleSystem[] offroadDustParticles;
        [SerializeField] private SurfaceDetector surfaceDetector;

        private void Update()
        {
            CheckOffroadDustParticles();
        }

        private void CheckOffroadDustParticles()
        {
            if ((surfaceDetector.CurrentSurface.layerMask.value & ResourceManager.Instance.offroadLayer.value) != 0)
            {
                StartParticleEffect(offroadDustParticles);
            }
            else
            {
                FadeOutAndDisableParticleEffect(offroadDustParticles);
            }
        }

        private void StartParticleEffect(ParticleSystem[] particles)
        {
            foreach (var particle in particles)
            {
                if (!particle.isPlaying)
                    particle.Play(withChildren: true);
            }
        }

        private void FadeOutAndDisableParticleEffect(ParticleSystem[] particles)
        {
            foreach (var particle in particles)
            {
                if (particle.isPlaying)
                    particle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}