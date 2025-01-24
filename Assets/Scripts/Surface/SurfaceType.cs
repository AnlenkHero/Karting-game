using UnityEngine;

namespace Kart.Surface
{
    [CreateAssetMenu(fileName = "New Surface", menuName = "Kart/Surface Type")]
    public class SurfaceType : ScriptableObject
    {
        public string surfaceName;
        public float forwardFriction = 1.0f;
        public float sidewaysFriction = 1.0f;
        public float frictionMultiplier = 1.0f;
        public float brakeMultiplier = 1.0f;
        public float slowdownMultiplier = 1.0f;
        public float steeringSensitivityMultiplier = 1.0f;

        public AudioClip audioClip;
        
        public float smoothTime = 1f;
        
        public bool isContinuousEffect;
        public SurfaceBehavior customBehavior;
    }
}