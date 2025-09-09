using Fusion;
using Kart.Project_Files.Scripts.Managers;
using Kart.Project_Files.Scripts.Surface;
using Kart.Project_Files.Scripts.VFX;
using UnityEngine;

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
                ParticleSystemHelper.StartParticleEffect(offroadDustParticles);
            }
            else
            {
                ParticleSystemHelper.FadeOutAndDisableParticleEffect(offroadDustParticles);
            }
        }


    }
}