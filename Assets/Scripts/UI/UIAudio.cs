using Kart.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kart.UI
{
    public class UIAudio : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        [SerializeField] private Button button;

        private void Awake()
        {
            button.onClick.AddListener(() => AudioManager.Instance.PlayUI("clickUI"));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button || button.interactable)
                AudioManager.Instance.PlayUI("hoverUI");
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (eventData is PointerEventData) return;

            if (!button || button.interactable)
                AudioManager.Instance.PlayUI("hoverUI");
        }
    }
}