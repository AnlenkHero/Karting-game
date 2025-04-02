using Kart.Project_Files.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Audio
{
    [RequireComponent(typeof(Toggle))]
    public class UIToggleAudio : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        
        private void Reset()
        {
            toggle = GetComponent<Toggle>();
        }

        private void Awake()
        {
            if (GetComponentInParent<Dropdown>() != null)
            {
                return;
            }
            
            if (toggle == null)
            {
                toggle = GetComponent<Toggle>();
            }
            toggle.onValueChanged.AddListener((bool value) => AudioManager.Instance.PlayUI("clickUI"));
        }
    }
}