using System.Linq;
using Fusion;
using Fusion.Addons.Physics;
using Kart.Project_Files.Scripts.Managers.Game;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kart.Project_Files.Scripts.Controls
{
    public class CarResetter : NetworkBehaviour
    {
        [Header("Hold-to-Reset Settings")] [SerializeField]
        private float holdDuration = 3f;

        [SerializeField] private NetworkRigidbody3D networkRigidbody3D;
        [SerializeField] private KartController kartController;
        [SerializeField] private TextMeshProUGUI resetText;

        private PlayerInputActions _actions;
        private bool _isHolding;
        private float _holdTimer;
        private InputControl _lastResetControl;

        void Awake()
        {
            _actions = new PlayerInputActions();
            resetText.gameObject.SetActive(false);
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

        private void OnRespawnStarted(InputAction.CallbackContext ctx)
        {
            if (!Object.HasInputAuthority) return;

            _isHolding = true;
            _lastResetControl = ctx.control;
            UpdateResetLabel();

            resetText.gameObject.SetActive(true);
        }

        private void OnRespawnCanceled(InputAction.CallbackContext _)
        {
            _isHolding = false;
            _holdTimer = 0f;
            resetText.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!Object.HasInputAuthority || !_isHolding || GameManager.Instance.CurrentGameState != GameState.Running)
                return;

            _holdTimer += Time.deltaTime;
            if (_holdTimer >= holdDuration)
            {
                _holdTimer = 0f;
                _isHolding = false;
                resetText.gameObject.SetActive(false);

                int idx = GetNearestCheckpointIndex();
                RPC_RequestReset(idx);
            }
        }

        private void UpdateResetLabel()
        {
            bool usingGamepad = _lastResetControl?.device is Gamepad;

            var binding = _actions.Player.Respawn.bindings
                .Where(b => !b.isComposite)
                .First(b =>
                    usingGamepad
                        ? b.groups.Contains("Gamepad")
                        : b.groups.Contains("Keyboard&Mouse")
                );

            string glyph = InputControlPath.ToHumanReadableString(
                binding.effectivePath,
                InputControlPath.HumanReadableStringOptions.OmitDevice
            );

            resetText.text = $"Hold ({glyph}) to Reset";
        }

        private int GetNearestCheckpointIndex()
        {
            var cps = GameManager.Instance.currentTrack.resetCheckpoints;
            int bestI = -1;
            float bestSq = float.MaxValue;
            Vector3 me = transform.position;

            for (int i = 0; i < cps.Length; i++)
            {
                float d = (cps[i].position - me).sqrMagnitude;
                if (d < bestSq)
                {
                    bestSq = d;
                    bestI = i;
                }
            }

            return bestI;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestReset(int checkpointIndex)
        {
            if (checkpointIndex < 0) return;
            var cp = GameManager.Instance.currentTrack.resetCheckpoints[checkpointIndex];
            kartController.ResetSpeed();
            networkRigidbody3D.Teleport(cp.position, cp.rotation);
        }
    }
}