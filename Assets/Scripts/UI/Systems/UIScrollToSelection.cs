/// Credit zero3growlithe
/// sourced from: http://forum.unity3d.com/threads/scripts-useful-4-6-scripts-collection.264161/page-2#post-2011648

/*USAGE:
Simply place the script on the ScrollRect that contains the selectable children you will be scrolling
*/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kart.UI
{
    [RequireComponent(typeof(ScrollRect))]
    [AddComponentMenu("UI/Extensions/UIScrollToSelection")]
    public class UIScrollToSelection : MonoBehaviour
    {
        #region MEMBERS

        [Header("[ References ]")]
        [SerializeField,
         Tooltip("View (boundaries/mask) rect transform. Used to check if automatic scroll to selection is required.")]
        private RectTransform viewportRectTransform;

        [SerializeField, Tooltip("Scroll rect used to reach selected element.")]
        private ScrollRect targetScrollRect;

        [Header("[ Scrolling ]")] [SerializeField, Tooltip("Allow automatic scrolling only on these axes.")]
        private Axis scrollAxes = Axis.ANY;

        [SerializeField, Tooltip("MOVE_TOWARDS: stiff movement, LERP: smoothed out movement")]
        private ScrollMethod usedScrollMethod = ScrollMethod.MOVE_TOWARDS;

        [SerializeField] private float scrollSpeed = 50;

        [Space(5)]
        [SerializeField, Tooltip("Scroll speed used when element to select is out of \"JumpOffsetThreshold\" range")]
        private float endOfListJumpScrollSpeed = 150;

        [SerializeField, Range(0, 1),
         Tooltip(
             "If next element to scroll to is located over this screen percentage, use \"EndOfListJumpScrollSpeed\" to reach this element faster.")]
        private float jumpOffsetThreshold = 1;

        [Header("[ Input ]")] [SerializeField] private MouseButton cancelScrollMouseButtons = MouseButton.ANY;
        [SerializeField] private KeyCode[] cancelScrollKeys = new KeyCode[0];

        // INTERNAL - MEMBERS ONLY
        private Vector3[] viewRectCorners = new Vector3[4];
        private Vector3[] selectedElementCorners = new Vector3[4];

        #endregion

        #region PROPERTIES

        // REFERENCES
        public RectTransform ViewRectTransform
        {
            get { return viewportRectTransform; }
            set { viewportRectTransform = value; }
        }

        public ScrollRect TargetScrollRect
        {
            get { return targetScrollRect; }
            set { targetScrollRect = value; }
        }

        // SCROLLING
        public Axis ScrollAxes => scrollAxes;
        public ScrollMethod UsedScrollMethod => usedScrollMethod;
        public float ScrollSpeed => scrollSpeed;
        public float EndOfListJumpScrollSpeed => endOfListJumpScrollSpeed;
        public float JumpOffsetThreshold => jumpOffsetThreshold;

        // INPUT
        public MouseButton CancelScrollMouseButtons => cancelScrollMouseButtons;
        public KeyCode[] CancelScrollKeys => cancelScrollKeys;

        // VARIABLES
        private RectTransform scrollRectContentTransform;
        private GameObject lastCheckedSelection;

        // COROUTINES
        private Coroutine animationCoroutine;

        #endregion

        #region FUNCTIONS

        protected void Awake()
        {
            ValidateReferences();
        }

        protected void LateUpdate()
        {
            TryToScrollToSelection();
        }

        protected void Reset()
        {
            TargetScrollRect = gameObject.GetComponentInParent<ScrollRect>() ??
                               gameObject.GetComponentInChildren<ScrollRect>();
            ViewRectTransform = gameObject.GetComponent<RectTransform>();
        }

        private void ValidateReferences()
        {
            if (!targetScrollRect)
            {
                targetScrollRect = GetComponent<ScrollRect>();
            }

            if (!targetScrollRect)
            {
                Debug.LogError(
                    "[UIScrollToSelection] No ScrollRect found. Either attach this script to a ScrollRect or assign on in the 'Target Scroll Rect' property");
                gameObject.SetActive(false);
                return;
            }

            if (ViewRectTransform == null)
            {
                ViewRectTransform = TargetScrollRect.GetComponent<RectTransform>();
            }

            if (TargetScrollRect != null)
            {
                scrollRectContentTransform = TargetScrollRect.content;
            }

            if (EventSystem.current == null)
            {
                Debug.LogError(
                    "[UIScrollToSelection] Unity UI EventSystem not found. It is required to check current selected object.");
                gameObject.SetActive(false);
                return;
            }
        }

        private void TryToScrollToSelection()
        {
            // update references if selection changed
            GameObject selection = EventSystem.current.currentSelectedGameObject;

            if (selection == null || selection.activeInHierarchy == false || selection == lastCheckedSelection ||
                selection.transform.IsChildOf(transform) == false)
            {
                return;
            }

            RectTransform selectionRect = selection.GetComponent<RectTransform>();

            ViewRectTransform.GetWorldCorners(viewRectCorners);
            selectionRect.GetWorldCorners(selectedElementCorners);

            ScrollToSelection(selection);

            lastCheckedSelection = selection;
        }

        private void ScrollToSelection(GameObject selection)
        {
            if (selection == null)
                return;

            RectTransform selectionRect = selection.GetComponent<RectTransform>();
            if (selectionRect == null)
                return;


            ViewRectTransform.GetWorldCorners(viewRectCorners);
            selectionRect.GetWorldCorners(selectedElementCorners);


            float verticalMargin = 50f;

            Vector3 adjustedTopLeft = viewRectCorners[1];
            Vector3 adjustedTopRight = viewRectCorners[2];
            adjustedTopLeft.y -= verticalMargin;
            adjustedTopRight.y -= verticalMargin;

            Vector3 adjustedBottomLeft = viewRectCorners[0];
            Vector3 adjustedBottomRight = viewRectCorners[3];
            adjustedBottomLeft.y += verticalMargin;
            adjustedBottomRight.y += verticalMargin;


            Vector3[] adjustedViewCorners = new Vector3[4]
            {
                adjustedBottomLeft,
                adjustedTopLeft,
                adjustedTopRight,
                adjustedBottomRight
            };

            float offsetX = 0;
            if (selectedElementCorners[0].x < adjustedViewCorners[0].x)
            {
                offsetX = selectedElementCorners[0].x - adjustedViewCorners[0].x;
            }
            else if (selectedElementCorners[2].x > adjustedViewCorners[2].x)
            {
                offsetX = selectedElementCorners[2].x - adjustedViewCorners[2].x;
            }

            float offsetY = 0;
            if (selectedElementCorners[1].y > adjustedViewCorners[1].y)
            {
                offsetY = selectedElementCorners[1].y - adjustedViewCorners[1].y;
            }
            else if (selectedElementCorners[0].y < adjustedViewCorners[0].y)
            {
                offsetY = selectedElementCorners[0].y - adjustedViewCorners[0].y;
            }

            Vector2 offsetToSelection = new Vector2(offsetX, offsetY);

            float finalScrollSpeed = ScrollSpeed;
            if ((Mathf.Abs(offsetX) / Screen.width >= JumpOffsetThreshold) ||
                (Mathf.Abs(offsetY) / Screen.height >= JumpOffsetThreshold))
            {
                finalScrollSpeed = EndOfListJumpScrollSpeed;
            }

            Vector2 targetPosition = (Vector2)scrollRectContentTransform.localPosition - offsetToSelection;

            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(ScrollToPosition(targetPosition, finalScrollSpeed));
        }


        private IEnumerator ScrollToPosition(Vector2 targetPosition, float speed)
        {
            Vector3 startPosition = scrollRectContentTransform.localPosition;

            // cancel movement on axes not specified in ScrollAxes mask
            targetPosition.x = ((ScrollAxes | Axis.HORIZONTAL) == ScrollAxes) ? targetPosition.x : startPosition.x;
            targetPosition.y = ((ScrollAxes | Axis.VERTICAL) == ScrollAxes) ? targetPosition.y : startPosition.y;

            // move to target position
            Vector2 currentPosition2D = startPosition;
            float horizontalSpeed = (Screen.width / Screen.dpi) * speed;
            float verticalSpeed = (Screen.height / Screen.dpi) * speed;

            while (currentPosition2D != targetPosition && CheckIfScrollInterrupted() == false)
            {
                currentPosition2D.x = MoveTowardsValue(currentPosition2D.x, targetPosition.x, horizontalSpeed,
                    UsedScrollMethod);
                currentPosition2D.y =
                    MoveTowardsValue(currentPosition2D.y, targetPosition.y, verticalSpeed, UsedScrollMethod);

                scrollRectContentTransform.localPosition = currentPosition2D;

                yield return null;
            }

            scrollRectContentTransform.localPosition = currentPosition2D;
        }

        private bool CheckIfScrollInterrupted()
        {
            bool mouseButtonClicked = false;

            // check mouse buttons
            switch (CancelScrollMouseButtons)
            {
                case MouseButton.LEFT:
                    mouseButtonClicked |= Input.GetMouseButtonDown(0);
                    break;
                case MouseButton.RIGHT:
                    mouseButtonClicked |= Input.GetMouseButtonDown(1);
                    break;
                case MouseButton.MIDDLE:
                    mouseButtonClicked |= Input.GetMouseButtonDown(2);
                    break;
            }

            if (mouseButtonClicked == true)
            {
                return true;
            }

            // check keyboard buttons
            for (int i = 0; i < CancelScrollKeys.Length; i++)
            {
                if (Input.GetKeyDown(CancelScrollKeys[i]) == true)
                {
                    return true;
                }
            }

            return false;
        }

        private float MoveTowardsValue(float from, float to, float delta, ScrollMethod method)
        {
            switch (method)
            {
                case ScrollMethod.MOVE_TOWARDS:
                    return Mathf.MoveTowards(from, to, delta * Time.unscaledDeltaTime);
                case ScrollMethod.LERP:
                    return Mathf.Lerp(from, to, delta * Time.unscaledDeltaTime);
                default:
                    return from;
            }
        }

        #endregion

        #region CLASS_ENUMS

        [Flags]
        public enum Axis
        {
            NONE = 0x00000000,
            HORIZONTAL = 0x00000001,
            VERTICAL = 0x00000010,
            ANY = 0x00000011
        }

        [Flags]
        public enum MouseButton
        {
            NONE = 0x00000000,
            LEFT = 0x00000001,
            RIGHT = 0x00000010,
            MIDDLE = 0x00000100,
            ANY = 0x00000111
        }

        public enum ScrollMethod
        {
            MOVE_TOWARDS,
            LERP
        }

        #endregion
    }
}