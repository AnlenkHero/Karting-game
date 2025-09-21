using System.Collections;
using System.Collections.Generic;
using Fusion;
using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.TrackPackage;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kart.Project_Files.Scripts.Animations.TerrainRaceTrack
{
    public class LavaSphere : NetworkBehaviour
    {
        private static readonly int Lava1 = Animator.StringToHash("Lava1");

        [Header("Timing")]
        [SerializeField] private float minDelay = 15f;
        [SerializeField] private float maxDelay = 30f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        [Header("Hit Settings")]
        [SerializeField] private float hitImpulse = 2200f;
        [SerializeField] private float upBoost = 350f;
        [SerializeField] private float hitCooldown = 0.25f;

        private Vector3 _startPosition;
        private Coroutine _lavaRoutine;
        private readonly Dictionary<KartController, float> _lastHitTime = new();

        private void Awake()
        {
            _startPosition = transform.position;
            Debug.Log($"[LavaSphere] Awake. StartPos={_startPosition}, HasStateAuthority={HasStateAuthority}");
        }

        public override void Spawned()
        {
            base.Spawned();
            Debug.Log($"[LavaSphere] Spawned. HasStateAuthority={HasStateAuthority}, Object.InputAuthority={Object.InputAuthority}");
            if (HasStateAuthority)
            {
                if (_lavaRoutine != null) StopCoroutine(_lavaRoutine);
                _lavaRoutine = StartCoroutine(LavaLoop());
            }
        }

        private IEnumerator LavaLoop()
        {
            Debug.Log("[LavaSphere] Lava loop coroutine STARTED");
            while (true)
            {
                var randomDelay = Random.Range(minDelay, maxDelay);
                Debug.Log($"[LavaSphere] Waiting {randomDelay:0.00}s before trigger");
                yield return new WaitForSeconds(randomDelay);
                RPC_PlayLavaTrigger();
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_PlayLavaTrigger()
        {
            Debug.Log("[LavaSphere] RPC_PlayLavaTrigger");
            if (animator) animator.SetTrigger(Lava1);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!HasStateAuthority) return;
            var kart = other.collider.GetComponentInParent<KartController>();
            if (kart == null) return;
            ApplyConstantPushForce(kart, other);
        }
        

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[LavaSphere] OnTriggerEnter with {other.name}");
            if (!other.gameObject.GetComponent<Deadzone>()) return;
            if (HasStateAuthority)
            {
                if (_lavaRoutine != null) StopCoroutine(_lavaRoutine);
                _lavaRoutine = StartCoroutine(LavaLoop());
            }
        }

        private void ApplyConstantPushForce(KartController kart, Collision other)
        {
            var contact = other.GetContact(0);
            Vector3 n = contact.normal.sqrMagnitude > 0f ? contact.normal.normalized : Vector3.up;
            Vector3 pushDir = -n;
            Vector3 force = pushDir * hitImpulse + Vector3.up * upBoost;
            kart.ApplyPushForce(force);
            Debug.Log($"[LavaSphere] Pushed {kart.name} force={force}");
        }
    }
}
