using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    [System.Serializable]
    public class SteeringButtonData : MonoBehaviour
    {
        [Tooltip("A human‐readable name for debugging or tooling.")]
        public string buttonName;

        [Tooltip("The world‐space Z-rotation (in degrees) at which this button should be the CENTER/selected button.")]
        [Range(0f, 360f)]
        public float targetAngle;

        [Tooltip("Instead of a UnityEvent, drag in a Button prefab or reference that has its own Image + onClick listeners assigned.")]
        public Button button;

        [Tooltip("If false, this entry never shows up in the rotation cycle.")]
        public bool isVisible;
    }
}