using System;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.ModeStrategy.LapStrategy;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kart.Project_Files.Scripts.Controls
{
    public class SpectatorController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI spectatorText;
        private KartCameraController _currentCinemachineCamera;
        private KartUI _currentKartUI;
        private bool _canSpectate;
        private int currentCameraIndex = 0;

        private void OnEnable()
        {
            LapsGameModeStrategy.OnLocalPlayerFinished += SetSpectateState;
        }

        private void OnDisable()
        {
            LapsGameModeStrategy.OnLocalPlayerFinished -= SetSpectateState;
        }

        private void SetSpectateState()
        {
            _canSpectate = true;
            spectatorText.gameObject.SetActive(true);
            SetupCamera();
        }

        private void SetupCamera()
        {
            if (_currentCinemachineCamera == null && GameManager.Players.Count != 0)
            {
                _currentCinemachineCamera = GameManager.Players[currentCameraIndex].cameraController;
                _currentKartUI = GameManager.Players[currentCameraIndex].kartUI;
                _currentKartUI.ShowPlayerUI(false);
                _currentCinemachineCamera.SetupCamera();
                spectatorText.text = $"Spectating: {GameManager.Players[currentCameraIndex].KartName}";
            }
        }

        private void ChooseNextCamera()
        {
            if (GameManager.Players.Count == 0) return;
            
            _currentKartUI.ShowPlayerUI(true);
            _currentCinemachineCamera.DespawnCamera();
            
            currentCameraIndex = (currentCameraIndex + 1) % GameManager.Players.Count;
            
            _currentCinemachineCamera = GameManager.Players[currentCameraIndex].cameraController;
            _currentKartUI            = GameManager.Players[currentCameraIndex].kartUI;
            _currentKartUI.ShowPlayerUI(false);
            
            _currentCinemachineCamera.SetupCamera();
            spectatorText.text = $"Spectating: {GameManager.Players[currentCameraIndex].KartName}";
        }

        public void Update()
        {
            if (!_canSpectate || GameManager.Instance == null ||
                GameManager.Instance.CurrentGameState == GameState.Finished)
            {
                spectatorText.gameObject.SetActive(false);
                return;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) || !_currentCinemachineCamera && GameManager.Players.Count != 0)
            {
                ChooseNextCamera();
            }
        }
    }
}