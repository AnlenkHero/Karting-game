using UnityEngine;
using UnityEngine.Events;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    [System.Serializable]
    public class SteeringButtonData
    {
        [Tooltip("A human‐readable name for debugging or tooling.")]
        public string buttonName;

        [Tooltip("The icon or sprite you want to show when this button occupies a slot.")]
        public Sprite icon;

        [Tooltip("The world‐space Z-rotation (in degrees) at which this button should be the CENTER/selected button.")]
        [Range(0f, 360f)]
        public float targetAngle;

        [Tooltip("What should happen when this button is clicked/activated.")]
        public UnityEvent onClick;
    }
}