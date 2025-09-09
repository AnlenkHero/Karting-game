using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Animations.Dropdown
{
    [RequireComponent(typeof(ScrollRect))]
    public class FixedHandleSize : MonoBehaviour
    {
        [Range(0,1)] public float handleSize = 0.5f;

        ScrollRect _scrollRect;
        Scrollbar _vert, _horiz;

        void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _vert  = _scrollRect.verticalScrollbar;
            _horiz = _scrollRect.horizontalScrollbar;
        }

        void LateUpdate()
        {
            if (_vert  != null) _vert.size  = handleSize;
            if (_horiz != null) _horiz.size = handleSize;
        }
    }
}