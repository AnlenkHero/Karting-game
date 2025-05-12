using UnityEngine;

namespace Kart.Project_Files.Scripts.VFX
{
    public class SparkOnCollision : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private ParticleSystem[] sparkSystems;

        [Tooltip("Minimum scrape speed to emit sparks")] [SerializeField]
        private float minImpactSpeed = 2f;

        [Tooltip("How many sparks per contact point burst")] [SerializeField]
        private int burstCount = 8;

        [Tooltip("Multiplier for spark velocity")] [SerializeField]
        private float sparkSpeed = 3f;

        private void Start()
        {
            foreach (var sparkSystem in sparkSystems)
            {
                sparkSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void OnCollisionEnter(Collision collision) => HandleCollision(collision);
        private void OnCollisionStay(Collision collision) => HandleCollision(collision);

        private void HandleCollision(Collision collision)
        {
            if (rb.linearVelocity.magnitude < minImpactSpeed)
                return;

            foreach (var sparkSystem in sparkSystems)
            {
                ParticleSystemHelper.EmitAtContacts(
                    sparkSystem,
                    collision.contacts,
                    collision.relativeVelocity,
                    burstCount,
                    sparkSpeed
                );
            }
        }
    }
}