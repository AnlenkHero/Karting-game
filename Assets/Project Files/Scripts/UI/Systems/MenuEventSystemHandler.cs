using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    public class MenuEventSystemHandler : MonoBehaviour
    {
        [Header("Selectables")] public List<Selectable> selectables = new List<Selectable>();

        [Header("Animations")] [SerializeField]
        protected float _scaleSelectAnimationScale = 1.1f;

        [SerializeField] protected float _scaleDuration = 0.25f;

        protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();
        protected Tween _scaleUpTween;
        protected Tween _scaleDownTween;

        protected virtual void Awake()
        {
            // Store original scales & add event triggers
            foreach (Selectable selectable in selectables)
            {
                AddSelectionListeners(selectable);
                _scales.Add(selectable, selectable.transform.localScale);
            }
        }

        private void OnEnable()
        {
            // Reset scale each time this object is enabled
            foreach (var sel in selectables)
            {
                sel.transform.localScale = _scales[sel];
            }
        }

        private void OnDestroy()
        {
            // Kill any active tweens to avoid errors when objects are destroyed
            _scaleUpTween?.Kill(true);
            _scaleDownTween?.Kill(true);
        }

        protected virtual void AddSelectionListeners(Selectable selectable)
        {
            // Ensure there's an EventTrigger component on the same GameObject
            EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = selectable.gameObject.AddComponent<EventTrigger>();

            // SELECT event (usually triggered by keyboard/controller navigation)
            var selectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select
            };
            selectEntry.callback.AddListener(OnSelect);
            trigger.triggers.Add(selectEntry);

            // DESELECT event
            var deselectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Deselect
            };
            deselectEntry.callback.AddListener(OnDeselect);
            trigger.triggers.Add(deselectEntry);

            // POINTER ENTER (hover start)
            var pointerEnterEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            // Pass the 'selectable' via a lambda so we know which object is hovered
            pointerEnterEntry.callback.AddListener((BaseEventData data) => OnPointerEnter(data, selectable));
            trigger.triggers.Add(pointerEnterEntry);

            // POINTER EXIT (hover end)
            var pointerExitEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            pointerExitEntry.callback.AddListener((BaseEventData data) => OnPointerExit(data, selectable));
            trigger.triggers.Add(pointerExitEntry);
        }

        /// <summary>
        /// Called when a UI element is "selected" (e.g., via keyboard or controller focus).
        /// </summary>
        private void OnSelect(BaseEventData eventData)
        {
            Vector3 newScale = eventData.selectedObject.transform.localScale * _scaleSelectAnimationScale;
            _scaleUpTween = eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
        }

        /// <summary>
        /// Called when a UI element is "deselected."
        /// </summary>
        private void OnDeselect(BaseEventData eventData)
        {
            Selectable selectable = eventData.selectedObject.GetComponent<Selectable>();
            _scaleDownTween = selectable.transform.DOScale(_scales[selectable], _scaleDuration);
        }

        /// <summary>
        /// Called when the mouse pointer enters the Selectable's area (hover).
        /// </summary>
        private void OnPointerEnter(BaseEventData eventData, Selectable selectable)
        {
            PointerEventData pointerEventData = eventData as PointerEventData;
            if (pointerEventData != null)
            {
                Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
                if (sel == null)
                {
                    sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
                }

                pointerEventData.selectedObject = sel.gameObject;
            }
        }

        /// <summary>
        /// Called when the mouse pointer exits the Selectable's area (hover ends).
        /// </summary>
        private void OnPointerExit(BaseEventData eventData, Selectable selectable)
        {
            PointerEventData pointerEventData = eventData as PointerEventData;
            if(pointerEventData != null)
            {
                pointerEventData.selectedObject = null;
            }
        }
        
    }
}