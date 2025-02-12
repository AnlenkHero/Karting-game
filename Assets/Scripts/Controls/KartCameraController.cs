using Fusion;
using Kart.Controls;
using Unity.Cinemachine;
using UnityEngine;

public class KartCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 5f, -10f);
    [SerializeField] private float rotationDamping = 0.2f;
    [SerializeField] private KartController kartController;
    private float updateInterval = 0.3f;
    private float nextUpdateTime = 0f;
    private Transform kartTransform;
    private Rigidbody kartRigidbody;


    public void SetupCamera()
    {
        kartTransform = GetComponent<Transform>();
        kartRigidbody = GetComponent<Rigidbody>();

        playerCamera.gameObject.SetActive(true);
        cinemachineCamera.gameObject.SetActive(true);
        Camera.main.gameObject.SetActive(false);

        ConfigureCinemachine();
    }

    private void ConfigureCinemachine()
    {
        // Find the new Follow and Aim components
    }

    private void LateUpdate()
    {
    }
}