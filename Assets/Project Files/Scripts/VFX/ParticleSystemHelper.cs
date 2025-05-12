using UnityEngine;

namespace Kart.Project_Files.Scripts.VFX
{
    public static class ParticleSystemHelper
    {
        public static void EmitAtContacts(
            ParticleSystem ps,
            ContactPoint[] contacts,
            Vector3 incomingVelocity,
            int particlesPerContact = 5,
            float velocityMultiplier   = 1f)
        {
            if (ps == null || contacts == null || contacts.Length == 0)
                return;
            
            int mid     = contacts.Length / 2;
            int[] idxs  = { 0, mid, contacts.Length - 1 };

            foreach (int i in idxs)
            {
                var cp = contacts[i];
                Vector3 sprayDir = Vector3.Reflect(incomingVelocity, cp.normal).normalized;

                var emit = new ParticleSystem.EmitParams {
                    position             = cp.point,
                    velocity             = sprayDir * velocityMultiplier,
                    applyShapeToPosition = true
                };
                
                Quaternion look = Quaternion.LookRotation(cp.normal);
                look.ToAngleAxis(out float angleDeg, out Vector3 axis);

                emit.axisOfRotation    = axis;

                emit.rotation          = angleDeg * Mathf.Deg2Rad;


                ps.Emit(emit, particlesPerContact);
            }
        }
        public static void StartParticleEffect(ParticleSystem[] particles)
        {
            foreach (var particle in particles)
            {
                if (!particle.isPlaying)
                    particle.Play(withChildren: true);
            }
        }

        public static void StartParticleEffect(ParticleSystem particle)
        {
            if (!particle.isPlaying)
                particle.Play(withChildren: true);
        }

        public static void FadeOutAndDisableParticleEffect(ParticleSystem[] particles)
        {
            foreach (var particle in particles)
            {
                if (particle.isPlaying)
                    particle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public static void FadeOutAndDisableParticleEffect(ParticleSystem particle)
        {
            if (particle.isPlaying)
                particle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}