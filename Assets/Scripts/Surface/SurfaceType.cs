using UnityEngine;

namespace Kart.Surface
{
    [CreateAssetMenu(fileName = "New Surface", menuName = "Kart/Surface Type")]
    public class SurfaceType : ScriptableObject
    {
        public string surfaceName;
        public float frictionMultiplier = 1.0f;
        public float slowdownMultiplier = 1.0f;
        public float steeringSensitivityMultiplier = 1.0f;
        
        public SurfaceBehavior customBehavior;
    }
}