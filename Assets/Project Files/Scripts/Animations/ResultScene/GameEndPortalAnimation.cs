using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Managers.Interface;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Animations.ResultScene
{
    public class GameEndPortalAnimation : MonoBehaviour
    {
        private static readonly int TimeFromScript = Shader.PropertyToID("_TimeFromScript");
        public bool animationEnded;
        public Button button;
        [SerializeField] private Material portalMaterial;
        [SerializeField] private Image portalImage;
        [SerializeField] private float zeroEpsilon = 0.01f;
        private Material _matInstance;
        private bool _isReturning;
        private int _initialSign;

        private void Awake()
        {
            if (portalMaterial == null || portalImage == null)
            {
                Debug.LogError("Missing portalMaterial or portalImage!", this);
                return;
            }
            button.interactable = false;
            button.onClick.AddListener(EndAnimation);
            

            _matInstance = Instantiate(portalMaterial);
            portalImage.material = _matInstance;
        }

        private void Update()
        {
            float current = Mathf.Sin(Time.time);
            
            if (!animationEnded)
            {
                _matInstance.SetFloat(TimeFromScript, current);
                return;
            }
            
            if (!_isReturning)
            {
                _isReturning = true;
                _initialSign = current >= 0f ? 1 : -1;
            }

            if (!_isReturning) return;
            if (current * _initialSign <= 0f || Mathf.Abs(current) < zeroEpsilon)
            {
                _matInstance.SetFloat(TimeFromScript, 0f);
                MoveToMainMenu();
                return;
            }
                
            _matInstance.SetFloat(TimeFromScript, current);
        }

        private void EndAnimation()
        {
            animationEnded = true;
        }
        
        private void MoveToMainMenu()
        {
            GameLauncher.Instance.LeaveSession();
        }
    }
}