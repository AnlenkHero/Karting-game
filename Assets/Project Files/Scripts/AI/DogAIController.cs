using System.Collections;
using Fusion;
using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.Managers.Game; // <— added
using UnityEngine;

namespace Kart.Project_Files.Scripts.AI
{
    [RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(AudioSource))]
    public class DogAIControllerWithRigidbody : NetworkBehaviour
    {
        // private static readonly Collider[] SOverlapResults = new Collider[16]; // not needed anymore

        [Header("Waypoints & Timing")] 
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float idleMinTime = 3f;
        [SerializeField] private float idleMaxTime = 6f;
        [SerializeField] private float waypointPause = 1f;
        [SerializeField] private float waypointReachDistance = 2.5f;
        [SerializeField] private float waypointMaxSeekTime = 7f;

        [Header("Speeds & Rotation")] 
        [SerializeField] private float patrolSpeed = 1.5f;
        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float rotationSpeed = 360f;

        [Header("Detection & Attack")] 
        [SerializeField] private float detectionRadius = 8f;
        [SerializeField] private float chaseDuration = 5f;
        [SerializeField] private float barkDistanceThreshold = 3f;
        [SerializeField] private float attackCooldown = 3f;
        // [SerializeField] private LayerMask playerLayer; // not used anymore
        [SerializeField] private AudioClip barkClip;

        [Header("Stun on Collision")] 
        [SerializeField] private float collisionSpeedThreshold = 5f;
        [SerializeField] private float brakingDamping = 5f;
        [SerializeField] private float stunDuration = 10f;

        [Header("Anti-Stuck")] 
        [SerializeField] private float stuckSpeedThreshold = 0.15f;
        [SerializeField] private float stuckTimeout = 1.25f;
        [SerializeField] private float sideNudgeVelocity = 1.25f;
        [SerializeField] private float forwardNudgeVelocity = 0.75f;

        [SerializeField] private Collider boxCollider;
        [SerializeField] private Animator anim;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private AudioSource audioSrc;

        private Transform _currentTarget;
        private Transform _lastAttackTarget;
        private int _patrolIndex;
        private Coroutine _stateRoutine;
        private float _lastAttackEndTime = -Mathf.Infinity;
        private WaitForSeconds _pauseWait;
        private WaitForSeconds _stunWait;

        private enum State { Idle, Patrol, Attack, Stunned }
        private State _state = State.Idle;

        private bool _isMoving;
        private float _moveSpeed;
        private Vector3 _moveDirection;
        private float _stuckTimer;

        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);

            _pauseWait = new WaitForSeconds(waypointPause);
            _stunWait = new WaitForSeconds(stunDuration);

            rb.constraints = RigidbodyConstraints.FreezeRotationX
                             | RigidbodyConstraints.FreezeRotationZ
                             | RigidbodyConstraints.FreezePositionY;

            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.sleepThreshold = 0.0f;

            GoToIdle();
        }

        void Update()
        {
            if (_state == State.Attack || _state == State.Stunned ||
                !(Time.time >= _lastAttackEndTime + attackCooldown))
                return;

            if (!HasStateAuthority)
                return;

            // NEW: find nearest KartController within radius
            Transform nearestKart = FindNearestKartWithin(detectionRadius);

            if (nearestKart != null && nearestKart != _lastAttackTarget)
            {
                _currentTarget = nearestKart;
                RpcGoToState(State.Attack);
            }
        }

        // Finds the closest KartController within 'radius'. Returns its transform or null if none.
        private Transform FindNearestKartWithin(float radius)
        {
            if (GameManager.Players == null || GameManager.Players.Count == 0)
                return null;

            float bestDistSqr = radius * radius;
            Transform best = null;

            // Iterate all registered karts
            for (int i = 0; i < GameManager.Players.Count; i++)
            {
                var kart = GameManager.Players[i];
                if (kart == null) continue;

                // Optional: skip undrivable karts
                // if (!kart.canDrive) continue;

                Vector3 diff = kart.transform.position - transform.position;
                float d2 = diff.sqrMagnitude;
                if (d2 <= bestDistSqr)
                {
                    bestDistSqr = d2;
                    best = kart.transform;
                }
            }

            return best;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
                return;

            float dt = Runner.DeltaTime > 0 ? Runner.DeltaTime : Time.fixedDeltaTime;

            Vector3 currentVel = rb.linearVelocity;
            Vector3 horizontalVel = new Vector3(currentVel.x, 0f, currentVel.z);

            if (_isMoving)
            {
                rb.WakeUp();

                Vector3 dir = _moveDirection; dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    var targetRotation = Quaternion.LookRotation(dir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * dt);
                }

                Vector3 forward = transform.forward; forward.y = 0f; forward.Normalize();
                Vector3 desiredVel = forward * _moveSpeed;
                Vector3 velDiff = desiredVel - horizontalVel;
                rb.AddForce(velDiff, ForceMode.VelocityChange);

                float speed = horizontalVel.magnitude;
                if (speed < stuckSpeedThreshold && desiredVel.sqrMagnitude > 0.01f)
                {
                    _stuckTimer += dt;
                    if (_stuckTimer >= stuckTimeout)
                    {
                        Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;
                        float sign = Random.value < 0.5f ? -1f : 1f;
                        Vector3 escapeDeltaV = side * (sideNudgeVelocity * sign) + forward * forwardNudgeVelocity;
                        rb.AddForce(escapeDeltaV, ForceMode.VelocityChange);
                        _stuckTimer = 0f;
                    }
                }
                else
                {
                    _stuckTimer = 0f;
                }
            }
            else
            {
                Vector3 brakingForce = -horizontalVel * brakingDamping * dt;
                rb.AddForce(brakingForce, ForceMode.VelocityChange);
                _stuckTimer = 0f;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        void OnCollisionEnter(Collision col)
        {
            if (_state == State.Stunned) return;

            var hitKart = col.gameObject.GetComponent<KartController>();
            if (hitKart == null) return;

            if (col.relativeVelocity.sqrMagnitude >= collisionSpeedThreshold * collisionSpeedThreshold)
            {
                if (HasStateAuthority)
                    RpcGoToState(State.Stunned);
            }
        }

        void GoToIdle()
        {
            if (HasStateAuthority)
                RpcGoToState(State.Idle);
        }

        void GoToPatrol()
        {
            if (HasStateAuthority)
                RpcGoToState(State.Patrol);
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void RpcGoToState(State newState)
        {
            if (_stateRoutine != null) StopCoroutine(_stateRoutine);
            _state = newState;
            _isMoving = false;
            _stuckTimer = 0f;

            _stateRoutine = _state switch
            {
                State.Idle    => StartCoroutine(IdleRoutine()),
                State.Patrol  => StartCoroutine(PatrolRoutine()),
                State.Attack  => StartCoroutine(AttackRoutine()),
                State.Stunned => StartCoroutine(StunnedRoutine()),
                _             => _stateRoutine
            };
        }

        IEnumerator IdleRoutine()
        {
            _patrolIndex = 0;
            if (HasStateAuthority) RpcMakeAChoice();
            yield return new WaitForSeconds(Random.Range(idleMinTime, idleMaxTime));
            GoToPatrol();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcMakeAChoice()
        {
            int choice = Random.Range(0, 5);
            switch (choice)
            {
                case 0: if (HasStateAuthority) Rpc_SetAnimationStateName("Breathing"); break;
                case 1: if (HasStateAuthority) Rpc_SetAnimationStateName("WigglingTail"); break;
                case 2: if (HasStateAuthority) Rpc_SetAnimationStateName("SittingStart"); break;
                case 3: if (HasStateAuthority) Rpc_SetAnimationStateName("EatingStart"); break;
                default: GoToPatrol(); break;
            }
        }

        IEnumerator PatrolRoutine()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                GoToIdle();
                yield break;
            }

            if (HasStateAuthority) Rpc_SetAnimationStateName(_patrolIndex % 2 == 0 ? "Walking01" : "Walking02");

            if (HasStateAuthority)
            {
                _moveSpeed = patrolSpeed;
                _isMoving = true;

                float timer = 0f;
                Transform target = patrolPoints[_patrolIndex];

                while (true)
                {
                    _moveDirection = target.position - transform.position;

                    if (_moveDirection.sqrMagnitude <= (waypointReachDistance * waypointReachDistance))
                        break;

                    timer += Time.deltaTime;
                    if (timer >= waypointMaxSeekTime)
                        break;

                    yield return null;
                }

                _isMoving = false;
            }

            if (HasStateAuthority)
                Rpc_SetAnimationStateName(Random.Range(0, 4) switch
                {
                    0 => "Breathing",
                    1 => "WigglingTail",
                    2 => "SittingStart",
                    _ => "EatingStart"
                });

            yield return _pauseWait;

            if (_patrolIndex >= patrolPoints.Length - 1)
                GoToIdle();
            else
            {
                _patrolIndex++;
                GoToPatrol();
            }
        }

        IEnumerator AttackRoutine()
        {
            if (HasStateAuthority) Rpc_SetAnimationStateName("Running");

            float timer = 0f;
            bool gotClose = false;

            if (HasStateAuthority)
            {
                _moveSpeed = chaseSpeed;
                _isMoving = true;
            }

            while (timer < chaseDuration && _currentTarget != null)
            {
                if (HasStateAuthority)
                    _moveDirection = _currentTarget.position - transform.position;

                float dist = _currentTarget != null
                    ? Vector3.Distance(transform.position, _currentTarget.position)
                    : float.MaxValue;

                if (dist <= barkDistanceThreshold)
                {
                    gotClose = true;
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            _isMoving = false;

            int barkCount = gotClose ? Random.Range(1, 4) : 1;
            for (int i = 0; i < barkCount; i++)
            {
                if (HasStateAuthority) Rpc_SetAnimationStateName("AngryStart");
                if (audioSrc != null && barkClip != null) audioSrc.PlayOneShot(barkClip);
                yield return new WaitForSeconds(1f);
            }

            _lastAttackTarget = _currentTarget;
            _lastAttackEndTime = Time.time;
            _currentTarget = null;

            GoToPatrol();
        }

        IEnumerator StunnedRoutine()
        {
            if (boxCollider != null) boxCollider.isTrigger = true;
            _currentTarget = null;
            _lastAttackEndTime = Time.time;
            _isMoving = false;

            rb.linearVelocity = Vector3.zero;

            if (HasStateAuthority) Rpc_SetAnimationStateName("SittingStart");
            yield return _stunWait;

            if (boxCollider != null) boxCollider.isTrigger = false;
            GoToIdle();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_SetAnimationStateName(string newName)
        {
            anim.CrossFade(newName, 0.1f);
        }
    }
}
