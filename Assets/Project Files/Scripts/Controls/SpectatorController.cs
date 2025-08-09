using System.Collections;
using System.Linq;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.ModeStrategy.LapStrategy;
using TMPro;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Controls
{
    public class SpectatorController : MonoBehaviour
    {
        public bool canSpectate;
        [SerializeField] private TextMeshProUGUI spectatorText;
        private KartCameraController _currentCinemachineCamera;
        private KartUI _currentKartUI;
        private int _currentCameraIndex = -1;
        public string CurrentSpectatedPlayerId { get; private set; }
        private PlayerInputActions _playerInputActions;
        private Coroutine _autoSpectateRoutine;

        private void Awake()
        {
            _playerInputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            LapsGameModeStrategy.OnLocalPlayerFinished += OnLocalFinished;
            _playerInputActions.UI.Navigate.performed += OnNavigate;
            _playerInputActions.Enable();
        }

        private void OnDisable()
        {
            LapsGameModeStrategy.OnLocalPlayerFinished -= OnLocalFinished;
            _playerInputActions.UI.Navigate.performed -= OnNavigate;
            _playerInputActions.Disable();
        }

        private void OnDestroy()
        {
            LapsGameModeStrategy.OnLocalPlayerFinished -= OnLocalFinished;
            _playerInputActions.UI.Navigate.performed -= OnNavigate;
            _playerInputActions.Disable();
        }

        private void OnLocalFinished()
        {
            canSpectate = true;
            spectatorText.gameObject.SetActive(true);

            if (_autoSpectateRoutine != null) StopCoroutine(_autoSpectateRoutine);
            _autoSpectateRoutine = StartCoroutine(AutoSpectateNextFrame());
        }

        private IEnumerator AutoSpectateNextFrame()
        {
            yield return null;

            var local = KartController.LocalKartController;
            
            float timeout = 2f, t = 0f;
            while (!HasSpectatableTargets(local))
            {
                t += Time.unscaledDeltaTime;
                if (t > timeout) break;
                yield return null;
            }

            int idx = GetFirstSpectatableIndex(local);
            if (idx >= 0)
                InternalSwitchToCamera(idx);
        }

        private static bool IsValidTarget(KartController k, KartController exclude)
        {
            if (k == null || k == exclude) return false;
            if (k.Object == null) return false;
            if (k.cameraController == null || k.kartUI == null) return false;
            return true;
        }

        private bool HasSpectatableTargets(KartController exclude)
        {
            if (GameManager.Instance == null || GameManager.Players.Count == 0) return false;
            return GameManager.Players.Any(k => IsValidTarget(k, exclude));
        }

        private int GetFirstSpectatableIndex(KartController exclude)
        {
            if (GameManager.Instance == null || GameManager.Players.Count == 0) return -1;
            for (int i = 0; i < GameManager.Players.Count; i++)
                if (IsValidTarget(GameManager.Players[i], exclude))
                    return i;
            return -1;
        }

        private void Update()
        {
            if (!canSpectate || GameManager.Instance == null ||
                GameManager.Instance.CurrentGameState == GameState.Finished)
            {
                spectatorText.gameObject.SetActive(false);
            }
        }

        private void OnNavigate(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            if (!canSpectate || GameManager.Instance == null ||
                GameManager.Instance.CurrentGameState == GameState.Finished ||
                GameManager.Players.Count == 0)
                return;

            var dir = context.ReadValue<Vector2>();

            if (dir.x > 0)
                SwitchToCamera((_currentCameraIndex + 1 + GameManager.Players.Count) % GameManager.Players.Count);
            else if (dir.x < 0)
                SwitchToCamera((_currentCameraIndex - 1 + GameManager.Players.Count) % GameManager.Players.Count);
        }

        private void SwitchToCamera(int newIndex)
        {
            StartCoroutine(SwitchWhenReady(newIndex));
        }

        private IEnumerator SwitchWhenReady(int newIndex)
        {
            yield return null;

            if (GameManager.Instance == null || GameManager.Players.Count == 0) yield break;

            newIndex = Mathf.Clamp(newIndex, 0, GameManager.Players.Count - 1);
            
            var local = KartController.LocalKartController;
            int tries = GameManager.Players.Count;
            int idx = newIndex;
            while (tries-- > 0 && !IsValidTarget(GameManager.Players[idx], local))
                idx = (idx + 1) % GameManager.Players.Count;

            if (!IsValidTarget(GameManager.Players[idx], local)) yield break;

            InternalSwitchToCamera(idx);
        }

        private void InternalSwitchToCamera(int idx)
        {
            if (_currentCinemachineCamera != null)
                _currentKartUI?.ShowPlayerUI(true);

            _currentCameraIndex = idx;
            var target = GameManager.Players[_currentCameraIndex];

            _currentCinemachineCamera = target.cameraController;
            _currentKartUI            = target.kartUI;
            
            _currentKartUI.ShowPlayerUI(false);

            SetSpectatedPlayerId(target);
            
            if (MinimapController.Instance != null)
                MinimapController.Instance.ChangeFollowedWorldObject(_currentKartUI.MinimapWorldObjectRef);
            
            _currentCinemachineCamera.SetupCamera();

            spectatorText.text = $"Spectating: {target.KartName}";
        }

        private void SetSpectatedPlayerId(KartController kart)
        {
            var rp = RoomPlayer.Players.First(p => p.Kart == kart);
            CurrentSpectatedPlayerId = rp.Id.ToString();
        }
    }
}
