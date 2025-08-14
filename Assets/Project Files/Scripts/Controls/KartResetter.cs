using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Addons.Physics;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.TrackPackage;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kart.Project_Files.Scripts.Controls
{
    public class KartResetter : NetworkBehaviour
    {
        [Header("Manual Hold-to-Reset")] 
        [SerializeField] private float holdDuration = 3f;

        [Header("Auto Wrong-Way (uses currentResetIdx)")] 
        [SerializeField] private bool autoResetOnWrongWay = true;
        [SerializeField] private float wrongWayLeadIn = 5f;
        [SerializeField] private float wrongWayCountdown = 3f;
        [SerializeField] private float faceBiasMargin = 0.12f;
        [SerializeField] private float faceMinDot = 0.35f;
        [SerializeField] private float startGraceSeconds = 0.5f;

        [Header("Auto 'Stuck' (hint only)")] 
        [SerializeField] private bool autoResetWhenStuck = true;
        [SerializeField] private float stuckMinPlanarSpeed = 0.5f;

        [Header("UI & Refs")] 
        [SerializeField] private NetworkRigidbody3D networkRigidbody3D;
        [SerializeField] private KartController kartController;
        [SerializeField] private TextMeshProUGUI resetText;

        [HideInInspector] public int currentResetIdx = -1;

        private PlayerInputActions _actions;
        private bool _isHolding;
        private float _holdTimer;
        private InputControl _lastResetControl;

        private float _wrongLeadTimer;
        private bool _inWrongCountdown;
        private float _wrongCountdownRemain;

        private float _stuckLeadTimer;
        private bool _stuckUiShown;
        private float _stuckUiStickRemain;

        private float _sinceSpawn;

        private ResetCheckpoint[] _orderedResets;
        private Dictionary<int, int> _indexToSlot;

        private void Awake()
        {
            _actions = new PlayerInputActions();
            if (resetText) resetText.gameObject.SetActive(false);
        }

        public override void Spawned()
        {
            base.Spawned();
            BuildResetCache();
            ResetRuntime();
        }

        void OnEnable()
        {
            _actions.Player.Enable();
            _actions.Player.Respawn.started += OnRespawnStarted;
            _actions.Player.Respawn.canceled += OnRespawnCanceled;
        }

        void OnDisable()
        {
            _actions.Player.Respawn.started -= OnRespawnStarted;
            _actions.Player.Respawn.canceled -= OnRespawnCanceled;
            _actions.Player.Disable();
        }

        private void ResetRuntime()
        {
            _isHolding = false;
            _holdTimer = 0f;

            _wrongLeadTimer = 0f;
            _inWrongCountdown = false;
            _wrongCountdownRemain = 0f;

            _stuckLeadTimer = 0f;
            _stuckUiShown = false;
            _stuckUiStickRemain = 0f;

            _sinceSpawn = 0f;

            if (resetText) resetText.gameObject.SetActive(false);
        }

        private void BuildResetCache()
        {
            var track = GameManager.Instance.currentTrack;
            var list = track ? track.resetCheckpoints : null;

            if (list == null || list.Length < 2)
            {
                _orderedResets = null;
                _indexToSlot = null;
                return;
            }

            _orderedResets = list.Where(cp => cp != null)
                .OrderBy(cp => cp.index)
                .ToArray();

            _indexToSlot = new Dictionary<int, int>(_orderedResets.Length);
            for (int s = 0; s < _orderedResets.Length; s++)
                _indexToSlot[_orderedResets[s].index] = s;
        }

        private void OnRespawnStarted(InputAction.CallbackContext ctx)
        {
            if (!Object.HasInputAuthority) return;
            _isHolding = true;
            _lastResetControl = ctx.control;
            UpdateResetLabel();
            if (resetText) resetText.gameObject.SetActive(true);
        }

        private void OnRespawnCanceled(InputAction.CallbackContext _)
        {
            _isHolding = false;
            _holdTimer = 0f;

            if (resetText && !_inWrongCountdown && !_stuckUiShown)
                resetText.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!Object.HasInputAuthority || GameManager.Instance.CurrentGameState != GameState.Running)
                return;

            if (_orderedResets == null || _indexToSlot == null)
                BuildResetCache();

            _sinceSpawn += Time.deltaTime;

            if (_isHolding)
            {
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= holdDuration)
                {
                    _holdTimer = 0f;
                    _isHolding = false;
                    if (currentResetIdx >= 0)
                        RPC_RequestReset(currentResetIdx);
                }
                else
                {
                    if (resetText && !resetText.gameObject.activeSelf)
                        resetText.gameObject.SetActive(true);
                    UpdateResetLabel();
                }
            }

            if (_orderedResets == null || _indexToSlot == null) return;
            if (autoResetOnWrongWay) TickWrongWayAuto();
            if (autoResetWhenStuck) TickStuckHint();
        }

        private void TickWrongWayAuto()
        {
            if (_sinceSpawn < startGraceSeconds || currentResetIdx < 0 ||
                !_indexToSlot.TryGetValue(currentResetIdx, out int slot))
            {
                ClearWrongWayState();
                return;
            }

            int n = _orderedResets.Length;
            if (n < 2)
            {
                ClearWrongWayState();
                return;
            }

            int nextSlot = (slot + 1) % n;
            int prevSlot = (slot - 1 + n) % n;

            Vector3 pos = transform.position;
            Vector3 head = PlanarNormalized(transform.forward);
            if (head.sqrMagnitude < 1e-6f)
            {
                ClearWrongWayState();
                return;
            }

            Vector3 toNext = PlanarNormalized(_orderedResets[nextSlot].transform.position - pos);
            Vector3 toPrev = PlanarNormalized(_orderedResets[prevSlot].transform.position - pos);
            if (toNext.sqrMagnitude < 1e-6f || toPrev.sqrMagnitude < 1e-6f)
            {
                ClearWrongWayState();
                return;
            }

            float dotNext = Vector3.Dot(head, toNext);
            float dotPrev = Vector3.Dot(head, toPrev);
            bool wrongThisFrame = (dotPrev > dotNext + faceBiasMargin) && (dotPrev >= faceMinDot);

            if (!wrongThisFrame)
            {
                ClearWrongWayState();
                return;
            }

            if (!_inWrongCountdown)
            {
                _wrongLeadTimer += Time.deltaTime;

                if (_wrongLeadTimer < wrongWayLeadIn)
                {
                    if (!_isHolding && resetText && resetText.gameObject.activeSelf)
                        resetText.gameObject.SetActive(false);
                    return;
                }

                _inWrongCountdown = true;
                _wrongCountdownRemain = wrongWayCountdown;
            }

            _wrongCountdownRemain -= Time.deltaTime;

            if (!_isHolding && resetText)
            {
                if (!resetText.gameObject.activeSelf)
                    resetText.gameObject.SetActive(true);
                resetText.text = $"Wrong way! Auto-reset in {_wrongCountdownRemain:F1}s";
            }

            if (!(_wrongCountdownRemain <= 0f)) return;
            if (resetText) resetText.gameObject.SetActive(false);
            _inWrongCountdown = false;
            _wrongLeadTimer = 0f;
            _wrongCountdownRemain = 0f;

            if (currentResetIdx >= 0)
                RPC_RequestReset(currentResetIdx);
        }

        private void ClearWrongWayState()
        {
            _wrongLeadTimer = 0f;

            bool wasCounting = _inWrongCountdown;
            _inWrongCountdown = false;
            _wrongCountdownRemain = 0f;

            if (wasCounting && resetText && !_isHolding && !_stuckUiShown)
                resetText.gameObject.SetActive(false);
        }

        private void TickStuckHint()
        {
            if (_sinceSpawn < startGraceSeconds || currentResetIdx < 0)
            {
                ClearStuckUI(forceHide: false);
                return;
            }

            if (!kartController.canDrive)
            {
                ClearStuckUI(forceHide: false);
                return;
            }

            bool trying = IsTryingToMove();

            float planarSpeed = GetPlanarSpeed();
            bool notMoving = planarSpeed < stuckMinPlanarSpeed;

            if (trying && notMoving)
            {
                if (!_stuckUiShown)
                {
                    _stuckLeadTimer += Time.deltaTime;
                    if (_stuckLeadTimer < wrongWayLeadIn)
                    {
                        if (!_isHolding && !_inWrongCountdown && resetText && resetText.gameObject.activeSelf)
                            resetText.gameObject.SetActive(false);
                        return;
                    }

                    _stuckUiShown = true;
                    _stuckUiStickRemain = wrongWayCountdown;
                }
                else
                {
                    _stuckUiStickRemain = wrongWayCountdown;
                }

                if (!_isHolding && !_inWrongCountdown && resetText)
                {
                    if (!resetText.gameObject.activeSelf)
                        resetText.gameObject.SetActive(true);
                    resetText.text = $"Stuck? Hold ({GetResetGlyph()}) to reset";
                }
            }
            else
            {
                if (!_stuckUiShown) return;
                _stuckUiStickRemain -= Time.deltaTime;
                if (_stuckUiStickRemain <= 0f)
                    ClearStuckUI(forceHide: true);
            }
        }

        private void ClearStuckUI(bool forceHide)
        {
            _stuckLeadTimer = 0f;

            bool wasShown = _stuckUiShown;
            _stuckUiShown = false;
            _stuckUiStickRemain = 0f;

            if (forceHide && wasShown && resetText && !_isHolding && !_inWrongCountdown)
                resetText.gameObject.SetActive(false);
        }

        private static Vector3 PlanarNormalized(Vector3 v)
        {
            v.y = 0f;
            float m = v.magnitude;
            return m > 1e-4f ? v / m : Vector3.zero;
        }

        private float GetPlanarSpeed()
        {
            if (!networkRigidbody3D) return 0f;

            var rb = networkRigidbody3D.Rigidbody;
            Vector3 v = rb ? (rb.linearVelocity) : Vector3.zero;
            return new Vector2(v.x, v.z).magnitude;
        }

        private bool IsTryingToMove()
        {
            var kb = Keyboard.current;
            bool keyAccel = (kb?.wKey.isPressed ?? false) || (kb?.upArrowKey.isPressed ?? false);
            bool keyReverse = (kb?.sKey.isPressed ?? false) || (kb?.downArrowKey.isPressed ?? false);

            var gp = Gamepad.current;
            bool padAccel = gp != null && (gp.rightTrigger.ReadValue() > 0.2f || gp.leftStick.ReadValue().y > 0.2f);
            bool padReverse = gp != null && (gp.leftTrigger.ReadValue() > 0.2f || gp.leftStick.ReadValue().y < -0.2f);

            return keyAccel || keyReverse || padAccel || padReverse;
        }

        private string GetResetGlyph()
        {
            bool usingGamepad = _lastResetControl?.device is Gamepad;

            var binding = _actions.Player.Respawn.bindings
                .Where(b => !b.isComposite)
                .FirstOrDefault(b => usingGamepad
                    ? (b.groups != null && b.groups.Contains("Gamepad"))
                    : (b.groups != null && b.groups.Contains("Keyboard&Mouse")));

            if (binding == default && _actions.Player.Respawn.bindings.Count > 0)
                binding = _actions.Player.Respawn.bindings.First(b => !b.isComposite);

            return binding != default
                ? InputControlPath.ToHumanReadableString(binding.effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice)
                : "Reset";
        }

        private void UpdateResetLabel()
        {
            if (!resetText) return;
            resetText.text = $"Hold ({GetResetGlyph()}) to Reset";
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestReset(int resetIndex)
        {
            if (_orderedResets == null || _indexToSlot == null)
                BuildResetCache();

            if (_indexToSlot == null || !_indexToSlot.TryGetValue(resetIndex, out int slot))
                return;

            var cp = _orderedResets![slot].transform;

            kartController.ResetSpeed();
            networkRigidbody3D.Teleport(cp.position, cp.rotation);

            _sinceSpawn = 0f;

            _wrongLeadTimer = 0f;
            _inWrongCountdown = false;
            _wrongCountdownRemain = 0f;

            _stuckLeadTimer = 0f;
            _stuckUiShown = false;
            _stuckUiStickRemain = 0f;

            if (resetText && !_isHolding)
                resetText.gameObject.SetActive(false);
        }
    }
}