using System;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.ModeStrategy.LapStrategy;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kart.Project_Files.Scripts.Controls
{
    public class SpectatorController : MonoBehaviour
    {
        private KartCameraController _currentCinemachineCamera;
        private KartCameraController _nextCinemachineCamera;
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
            SetupCamera();
        }

        private void SetupCamera()
        {
            if (_currentCinemachineCamera == null && GameManager.Players.Count != 0)
            {
                _currentCinemachineCamera = GameManager.Players[currentCameraIndex].cameraController;
                _currentCinemachineCamera.SetupCamera();
            }
        }

        private void ChooseNextCamera()
        {
            if (GameManager.Players.Count != 0)
            {
                currentCameraIndex++;
                if(currentCameraIndex >= GameManager.Players.Count)
                    currentCameraIndex = 0;

                
                _nextCinemachineCamera = GameManager.Players[currentCameraIndex].cameraController;
                _currentCinemachineCamera.DespawnCamera();
                _currentCinemachineCamera = _nextCinemachineCamera;
                _currentCinemachineCamera.SetupCamera();
            }
        }
        public void Update()
        {
            if (!_canSpectate)
                return;
            if (Input.GetKeyDown(KeyCode.Mouse0) || !_currentCinemachineCamera && GameManager.Players.Count != 0)
            {
                ChooseNextCamera();
            }
        }
    }
}