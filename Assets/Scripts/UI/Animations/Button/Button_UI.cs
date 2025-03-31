using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kart.UI
{
    public class Button_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        public Action ClickFunc = null;
        public Action MouseRightClickFunc = null;
        public Action MouseMiddleClickFunc = null;
        public Action MouseDownOnceFunc = null;
        public Action MouseUpFunc = null;
        public Action MouseOverOnceTooltipFunc = null;
        public Action MouseOutOnceTooltipFunc = null;
        public Action MouseOverOnceFunc = null;
        public Action MouseOutOnceFunc = null;
        public Action MouseOverFunc = null;
        public Action MouseOverPerSecFunc = null;
        public Action MouseUpdate = null;
        public Action<PointerEventData> OnPointerClickFunc;

        [Flags]
        public enum HoverBehaviour
        {
            None = 0,
            Change_Color = 1 << 0,
            Change_Image = 1 << 1,
            Change_SetActive = 1 << 2,
            Scale = 1 << 3,
            Rotate = 1 << 4,
            ImageOnClickColorPingPong = 1 << 5,
            TextOnClickColorPingPong = 1 << 6
        }

        public HoverBehaviour hoverBehaviourType = HoverBehaviour.None;
        private Action hoverBehaviourFunc_Enter, hoverBehaviourFunc_Exit;

        public Color hoverBehaviour_Color_Enter, hoverBehaviour_Color_Exit;
        public Image hoverBehaviour_Image;
        public Sprite hoverBehaviour_Sprite_Exit, hoverBehaviour_Sprite_Enter;
        public bool hoverBehaviour_Move = false;
        public Vector2 hoverBehaviour_Move_Amount = Vector2.zero;
        private Vector2 posExit, posEnter;
        public bool triggerMouseOutFuncOnClick = false;
        private bool mouseOver;
        private float mouseOverPerSecFuncTimer;
        private Sequence sequence;
        private Tween tween;
        private Action internalOnPointerEnterFunc, internalOnPointerExitFunc, internalOnPointerClickFunc;

        [SerializeField] private TextMeshProUGUI text;
        
        [Header("Scale Effect")] public Vector3 hoveredScale = new Vector3(1.1f, 1.1f, 1.1f);
        public float scaleAnimationDuration = 0.2f;
        private Vector3 originalScale;

        [Header("Rotation Effect")] public Vector3 hoveredRotation = new Vector3(0f, 0f, 180f);
        public float rotationAnimationDuration = 0.5f;
        private Vector3 originalRotation;

        [Header("Image Color Ping Pong Effect")] public Color targetImageColor = Color.yellow;
        public float imageColorAnimationDuration = 0.3f;
        public bool imageColorPingPongRepeat;
        private Color initialImageColor;
        private Sequence imageSequence;
        
       [Header("Text Color Ping Pong Effect")] public Color targetTextColor = Color.yellow;
       public float textColorAnimationDuration = 0.3f;
       public bool textColorPingPongRepeat;
       private Color initialTextColor;
       private Sequence textSequence;
       
#if SOUND_MANAGER
        public Sound_Manager.Sound mouseOverSound, mouseClickSound;
#endif
#if CURSOR_MANAGER
        public CursorManager.CursorType cursorMouseOver, cursorMouseOut;
#endif

        void Awake()
        {
            posExit = transform.localPosition;
            posEnter = (Vector2)transform.localPosition + hoverBehaviour_Move_Amount;
            originalScale = transform.localScale;
            originalRotation = transform.localEulerAngles;
            initialImageColor = hoverBehaviour_Image != null ? hoverBehaviour_Image.color : Color.white;
            initialTextColor = text != null ? text.color : Color.white;
            ConfigureHoverEffects();

#if SOUND_MANAGER
            // Sound Manager
            internalOnPointerEnterFunc +=
 () => { if (mouseOverSound != Sound_Manager.Sound.None) Sound_Manager.PlaySound(mouseOverSound); };
            internalOnPointerClickFunc +=
 () => { if (mouseClickSound != Sound_Manager.Sound.None) Sound_Manager.PlaySound(mouseClickSound); };
#endif

#if CURSOR_MANAGER
            // Cursor Manager
            internalOnPointerEnterFunc +=
 () => { if (cursorMouseOver != CursorManager.CursorType.None) CursorManager.SetCursor(cursorMouseOver); };
            internalOnPointerExitFunc +=
 () => { if (cursorMouseOut != CursorManager.CursorType.None) CursorManager.SetCursor(cursorMouseOut); };
#endif
        }

        void Update()
        {
            if (mouseOver)
            {
                if (MouseOverFunc != null) MouseOverFunc();
                mouseOverPerSecFuncTimer -= Time.unscaledDeltaTime;
                if (mouseOverPerSecFuncTimer <= 0)
                {
                    mouseOverPerSecFuncTimer += 1f;
                    if (MouseOverPerSecFunc != null) MouseOverPerSecFunc();
                }
            }

            if (MouseUpdate != null) MouseUpdate();
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (internalOnPointerEnterFunc != null) internalOnPointerEnterFunc();
            if (hoverBehaviour_Move) transform.localPosition = posEnter;
            if (hoverBehaviourFunc_Enter != null) hoverBehaviourFunc_Enter();
            if (MouseOverOnceFunc != null) MouseOverOnceFunc();
            if (MouseOverOnceTooltipFunc != null) MouseOverOnceTooltipFunc();
            mouseOver = true;
            mouseOverPerSecFuncTimer = 0f;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (internalOnPointerExitFunc != null) internalOnPointerExitFunc();
            if (hoverBehaviour_Move) transform.localPosition = posExit;
            if (hoverBehaviourFunc_Exit != null) hoverBehaviourFunc_Exit();
            if (MouseOutOnceFunc != null) MouseOutOnceFunc();
            if (MouseOutOnceTooltipFunc != null) MouseOutOnceTooltipFunc();
            mouseOver = false;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (internalOnPointerClickFunc != null) internalOnPointerClickFunc();
            if (OnPointerClickFunc != null) OnPointerClickFunc(eventData);
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (triggerMouseOutFuncOnClick)
                {
                    OnPointerExit(eventData);
                }

                if (ClickFunc != null) ClickFunc();
            }

            if (eventData.button == PointerEventData.InputButton.Right)
                if (MouseRightClickFunc != null)
                    MouseRightClickFunc();
            if (eventData.button == PointerEventData.InputButton.Middle)
                if (MouseMiddleClickFunc != null)
                    MouseMiddleClickFunc();
        }

        public void Manual_OnPointerExit()
        {
            OnPointerExit(null);
        }

        public bool IsMouseOver()
        {
            return mouseOver;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (MouseDownOnceFunc != null) MouseDownOnceFunc();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (MouseUpFunc != null) MouseUpFunc();
        }

        private void ConfigureHoverEffects()
        {
            hoverBehaviourFunc_Enter = () =>
            {
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Change_Color) && hoverBehaviour_Image != null)
                    hoverBehaviour_Image.color = hoverBehaviour_Color_Enter;
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Change_Image) && hoverBehaviour_Image != null)
                    hoverBehaviour_Image.sprite = hoverBehaviour_Sprite_Enter;
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Change_SetActive) && hoverBehaviour_Image != null)
                    hoverBehaviour_Image.gameObject.SetActive(true);
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Scale))
                    ApplyScaleEffect(true);
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Rotate))
                    ApplyRotationEffect(true);
                if(hoverBehaviourType.HasFlag(HoverBehaviour.ImageOnClickColorPingPong))
                    ApplyImageColorPingPongEffect(true);
                if(hoverBehaviourType.HasFlag(HoverBehaviour.TextOnClickColorPingPong))
                    ApplyTextColorPingPongEffect(true);
            };

            hoverBehaviourFunc_Exit = () =>
            {
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Change_Color) && hoverBehaviour_Image != null)
                    hoverBehaviour_Image.color = hoverBehaviour_Color_Exit;
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Change_Image) && hoverBehaviour_Image != null)
                    hoverBehaviour_Image.sprite = hoverBehaviour_Sprite_Exit;
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Change_SetActive) && hoverBehaviour_Image != null)
                    hoverBehaviour_Image.gameObject.SetActive(false);
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Scale))
                    ApplyScaleEffect(false);
                if (hoverBehaviourType.HasFlag(HoverBehaviour.Rotate))
                    ApplyRotationEffect(false);
                if(hoverBehaviourType.HasFlag(HoverBehaviour.ImageOnClickColorPingPong))
                    ApplyImageColorPingPongEffect(false);
                if(hoverBehaviourType.HasFlag(HoverBehaviour.TextOnClickColorPingPong))
                    ApplyTextColorPingPongEffect(false);
            };
        }

        private void ApplyScaleEffect(bool isHovered)
        {
            tween = Tween.Scale(transform, transform.localScale, isHovered ? hoveredScale : originalScale,
                scaleAnimationDuration, Ease.InOutCirc);
        }

        private void ApplyRotationEffect(bool isHovered)
        {
            tween = Tween.LocalEulerAngles(transform, transform.localEulerAngles,
                isHovered ? hoveredRotation : originalRotation, rotationAnimationDuration, Ease.InOutCirc);
        }
        
        private void ApplyPingPongColorEffect(Graphic graphic, Color initialColor, Color targetColor, float duration, bool isHovered, bool isPingPongRepeat, ref Sequence effectSequence)
        {
            if (graphic == null)
                return;
            
            effectSequence.Stop();

            if (isHovered)
            {
                effectSequence = Tween.Color(graphic, graphic.color, targetColor, duration)
                    .Group(Tween.Color(graphic, targetColor, initialColor, duration));
                if (isPingPongRepeat)
                {
                    effectSequence.SetRemainingCycles(-1);
                }
            }
            else
            {
                tween = Tween.Color(graphic, targetColor, initialColor, duration);
            }
        }

        private void ApplyImageColorPingPongEffect(bool isHovered)
        {
            ApplyPingPongColorEffect(hoverBehaviour_Image, initialImageColor, targetImageColor, imageColorAnimationDuration, isHovered, imageColorPingPongRepeat, ref imageSequence);
        }

        private void ApplyTextColorPingPongEffect(bool isHovered)
        {
            ApplyPingPongColorEffect(text, initialTextColor, targetTextColor, textColorAnimationDuration, isHovered, textColorPingPongRepeat, ref textSequence);
        }
    }
}