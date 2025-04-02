using Kart.Project_Files.Scripts.Controls;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Surface.SurfaceBehaviour
{
    [CreateAssetMenu(fileName = "SandSurfaceBehavior", menuName = "Kart/Surface Behaviors/Sand")]
    public class SandSurfaceBehavior : SurfaceBehavior
    {
        public ParticleSystem sandEffectPrefab;

        public override void ApplyBehavior(KartController kart, SurfaceType surface)
        {
            Debug.Log("Sand surface update");
        }
    }
}

