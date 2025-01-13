using Kart.Controls;
using UnityEngine;

namespace Kart.Surface.SurfaceBehaviour
{
    [CreateAssetMenu(fileName = "SandSurfaceBehavior", menuName = "Kart/Surface Behaviors/Sand")]
    public class SandSurfaceBehavior : SurfaceBehavior
    {
        public ParticleSystem sandEffectPrefab;
        public override bool IsContinuous { get; set; } = true;

        public override void ApplyBehavior(KartController kart, SurfaceType surface)
        {
                Debug.Log("NIGGA");
          //  kart.ReduceSpeed(surface.slowdownMultiplier * 0.8f);

/*            if (sandEffectPrefab)
            {
                var effect = Instantiate(sandEffectPrefab, kart.transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }*/
        }
    }
}