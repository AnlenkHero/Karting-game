using System.Collections;
using Fusion;
using Kart.Project_Files.Scripts.Controls;
using UnityEngine;

namespace Kart.Bublisher._3D_Stylized_Animated_Dogs_Kit.Scripts
{
    [RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(AudioSource))]
    public class DogAIControllerWithRigidbody : NetworkBehaviour
    {
        private static readonly Collider[] SOverlapResults = new Collider[16];

        [Header("Waypoints & Timing")] [SerializeField]
        private Transform[] patrolPoints;

        [SerializeField] private float idleMinTime = 3f;
        [SerializeField] private float idleMaxTime = 6f;
        [SerializeField] private float waypointPause = 1f;

        [Header("Speeds & Rotation")] [SerializeField]
        private float patrolSpeed = 1.5f;

        [SerializeField] private float chaseSpeed = 4f;
        [SerializeField] private float rotationSpeed = 360f;

        [Header("Detection & Attack")] [SerializeField]
        private float detectionRadius = 8f;

        [SerializeField] private float chaseDuration = 5f;
        [SerializeField] private float barkDistanceThreshold = 3f;
        [SerializeField] private float attackCooldown = 3f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private AudioClip barkClip;

        [Header("Stun on Collision")] [SerializeField]
        private float collisionSpeedThreshold = 5f;

        [SerializeField] private float brakingDamping = 5f;
        [SerializeField] private float stunDuration = 10f;

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
        private enum State
        {
            Idle,
            Patrol,
            Attack,
            Stunned
        }

        private State _state = State.Idle;

        private bool _isMoving;
        private float _moveSpeed;
        private Vector3 _moveDirection;

        public override void Spawned()
        { 
            _pauseWait = new WaitForSeconds(waypointPause);
            _stunWait  = new WaitForSeconds(stunDuration);
            rb.constraints = RigidbodyConstraints.FreezeRotationX
                             | RigidbodyConstraints.FreezeRotationZ
                             | RigidbodyConstraints.FreezePositionY;
        
            GoToIdle();
        }

        void Update()
        {
            if (_state == State.Attack || _state == State.Stunned ||
                !(Time.time >= _lastAttackEndTime + attackCooldown)) return;

            int hits = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, SOverlapResults, playerLayer);
            for (int i = 0; i < hits; i++)
            {
                var h = SOverlapResults[i].transform;
                if (h == _lastAttackTarget) continue;
                _currentTarget = h;
                RpcGoToState(State.Attack);
                break;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
                return;

            Vector3 currentVel = rb.linearVelocity;
            Vector3 horizontalVel = new Vector3(currentVel.x, 0f, currentVel.z);

            if (_isMoving)
            {
                Vector3 dir = _moveDirection;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    var targetRotation = Quaternion.LookRotation(dir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
                }

                Vector3 forward = transform.forward;
                forward.y = 0f;
                forward.Normalize();

                Vector3 desiredVel = forward * _moveSpeed;

                Vector3 velDiff = desiredVel - horizontalVel;

                rb.AddForce(velDiff, ForceMode.VelocityChange);
            }
            else
            {
                Vector3 brakingForce = -horizontalVel * brakingDamping * Time.fixedDeltaTime;
                rb.AddForce(brakingForce, ForceMode.VelocityChange);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        void OnCollisionEnter(Collision col)
        {
            if (_state != State.Stunned
                && col.gameObject.GetComponent<KartController>() != null
                && col.relativeVelocity.sqrMagnitude >= collisionSpeedThreshold)
            {
                RpcGoToState(State.Stunned);
            }
        }

        void GoToIdle() => RpcGoToState(State.Idle);
        void GoToPatrol() => RpcGoToState(State.Patrol);

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void RpcGoToState(State newState)
        {
            if (_stateRoutine != null) StopCoroutine(_stateRoutine);
            _state = newState;
            _isMoving = false;

            _stateRoutine = _state switch
            {
                State.Idle => StartCoroutine(IdleRoutine()),
                State.Patrol => StartCoroutine(PatrolRoutine()),
                State.Attack => StartCoroutine(AttackRoutine()),
                State.Stunned => StartCoroutine(StunnedRoutine()),
                _ => _stateRoutine
            };
        }

        IEnumerator IdleRoutine()
        {
            _patrolIndex = 0;
            RpcMakeAChoice();

            yield return new WaitForSeconds(Random.Range(idleMinTime, idleMaxTime));
            GoToPatrol();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcMakeAChoice()
        {
            int choice = Random.Range(0, 5);
            switch (choice)
            {
                case 0: Rpc_SetAnimationStateName("Breathing"); break;
                case 1: Rpc_SetAnimationStateName("WigglingTail"); break;
                case 2: Rpc_SetAnimationStateName("SittingStart"); break;
                case 3: Rpc_SetAnimationStateName("EatingStart"); break;
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

            Rpc_SetAnimationStateName(_patrolIndex % 2 == 0 ? "Walking01" : "Walking02");

            if (HasStateAuthority)
            {
                _moveSpeed = patrolSpeed;
                _isMoving = true;

                while (Vector3.Distance(transform.position, patrolPoints[_patrolIndex].position) > 3f)
                {
                    _moveDirection = patrolPoints[_patrolIndex].position - transform.position;
                    yield return null;
                }

                _isMoving = false;
            }

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
            Rpc_SetAnimationStateName("Running");

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

                float dist = Vector3.Distance(transform.position, _currentTarget.position);
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
                Rpc_SetAnimationStateName("AngryStart");
                if (barkClip != null) audioSrc.PlayOneShot(barkClip);
                yield return new WaitForSeconds(1f);
            }

            _lastAttackTarget = _currentTarget;
            _lastAttackEndTime = Time.time;
            _currentTarget = null;

            GoToPatrol();
        }

        IEnumerator StunnedRoutine()
        {
            boxCollider.isTrigger = true;
            _currentTarget = null;
            _lastAttackEndTime = Time.time;
            _isMoving = false;

            Rpc_SetAnimationStateName("SittingStart");
            yield return _stunWait;
            boxCollider.isTrigger = false;
            GoToIdle();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_SetAnimationStateName(string newName)
        {
            anim.CrossFade(newName, 0.1f);
        }
    }
}