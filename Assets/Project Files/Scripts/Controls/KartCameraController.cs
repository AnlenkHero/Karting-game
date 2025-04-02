using Unity.Cinemachine;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Controls
{
    public class KartCameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private Camera playerCamera;
    
        public void SetupCamera()
        {
            playerCamera.gameObject.SetActive(true);
            cinemachineCamera.gameObject.SetActive(true);
            if (Camera.main != null) Camera.main.gameObject.SetActive(false);
        }
    }
}