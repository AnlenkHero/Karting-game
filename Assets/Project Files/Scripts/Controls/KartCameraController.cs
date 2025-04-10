using Unity.Cinemachine;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Controls
{
    public class KartCameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCamera;


        public void SetupCamera()
        {
            cinemachineCamera.Priority = 100;
        }
    }
}