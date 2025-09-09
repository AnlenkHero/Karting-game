using Kart.Project_Files.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kart.Project_Files.Scripts.Audio
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class UITMPDropdownAudio : MonoBehaviour, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private TMP_Dropdown dropdown;
        
        private void Reset()
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }
        
        private void Awake()
        {
            if (dropdown == null)
                dropdown = GetComponent<TMP_Dropdown>();
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        
        private void OnDropdownValueChanged(int index)
        {
            AudioManager.Instance.PlayUI("clickUI");
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            AudioManager.Instance.PlayUI("clickUI");
        }
        
        public void OnSubmit(BaseEventData eventData)
        {
            AudioManager.Instance.PlayUI("clickUI");
        }
    }
}