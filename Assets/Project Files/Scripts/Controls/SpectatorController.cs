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
        private int _currentCameraIndex;
        public string CurrentSpectatedPlayerId { get; private set; }
        private PlayerInputActions _playerInputActions;

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
            SwitchToCamera((_currentCameraIndex + 1) % GameManager.Players.Count);
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
                GameManager.Instance.CurrentGameState == GameState.Finished || GameManager.Players.Count == 0)
            {
                return;
            }

            var direction = context.ReadValue<Vector2>();
            if (direction.x > 0)
            {
                SwitchToCamera((_currentCameraIndex + 1) % GameManager.Players.Count);
            }
            else if (direction.x < 0)
            {
                SwitchToCamera((_currentCameraIndex - 1 + GameManager.Players.Count) % GameManager.Players.Count);
            }
        }

        private void SwitchToCamera(int newIndex)
        {
            if (_currentCinemachineCamera != null)
            {
                _currentKartUI.ShowPlayerUI(true);
                _currentCinemachineCamera.DespawnCamera();
            }

            _currentCameraIndex = Mathf.Clamp(newIndex, 0, GameManager.Players.Count - 1);

            var target = GameManager.Players[_currentCameraIndex];
            _currentCinemachineCamera = target.cameraController;
            _currentKartUI = target.kartUI;

            _currentKartUI.ShowPlayerUI(false);
            SetSpectatedPlayerId(target);
            MinimapController.Instance.ChangeFollowedWorldObject(
                _currentKartUI.MinimapWorldObjectRef
            );
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