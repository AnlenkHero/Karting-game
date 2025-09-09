using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using Kart.Project_Files.Scripts.Managers.Game;

namespace Kart.Project_Files.Scripts.TrackPackage
{
    public class CameraAnimator : MonoBehaviour
    {
        [Header("Cinemachine Setup")]
        [SerializeField] private CinemachineBrain cinemachineBrain;
        [SerializeField] private CinemachineCamera[] cameras;

        [Header("Timing & Motion")]
        [SerializeField] private float holdDuration  = 1f;
        [SerializeField] private Vector3 slideOffset = new (5f, 0f, 0f);
        [SerializeField] private float slideDuration = 2f;
        
        private Vector3[] _originalPositions;
        private int      _currentIndex = -1;

        private void Awake()
        {
            _originalPositions = new Vector3[cameras.Length];
            for (int i = 0; i < cameras.Length; i++)
                _originalPositions[i] = cameras[i].transform.position;
        }

        private void OnEnable()
            => GameManager.Instance.OnGameEnded += StartAnimation;

        private void OnDisable()
            => GameManager.Instance.OnGameEnded -= StartAnimation;

        private void StartAnimation()
            => StartCoroutine(EndRaceLoop());

        private IEnumerator EndRaceLoop()
        {
            while (true)
            {
                _currentIndex = (_currentIndex + 1) % cameras.Length;
                var vc = cameras[_currentIndex];
                
                vc.transform.position = _originalPositions[_currentIndex];
                
                vc.Priority = 500;
                vc.gameObject.SetActive(true);
                
                yield return new WaitForSeconds(holdDuration);
                
                var start       = vc.transform.position;
                var worldOffset = vc.transform.TransformDirection(slideOffset);
                var end         = start + worldOffset;
                
                float t = 0f;
                while (t < slideDuration)
                {
                    t += Time.deltaTime;
                    vc.transform.position = Vector3.Lerp(start, end, t / slideDuration);
                    yield return null;
                }
                
                yield return new WaitForSeconds(holdDuration);
                
                vc.Priority = -300;
                vc.gameObject.SetActive(false);
            }
        }
    }
}
