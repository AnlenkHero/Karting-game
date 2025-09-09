using Kart.Project_Files.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Audio
{
    public class UIAudio : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        [SerializeField] private Selectable selectable;

        private void Reset()
        {
            selectable = GetComponent<Selectable>();
        }
        private void Awake()
        {
            if (selectable == null)
            {
                selectable = GetComponent<Selectable>();
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!selectable || selectable.interactable)
                AudioManager.Instance.PlayUI("hoverUI");
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (eventData is PointerEventData) return;

            if (!selectable || selectable.interactable)
                AudioManager.Instance.PlayUI("hoverUI");
        }
    }
}