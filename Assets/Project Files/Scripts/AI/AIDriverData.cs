using UnityEngine;

namespace Kart.Project_Files.Scripts.AI
{
    [CreateAssetMenu (fileName = "AIDriverData", menuName = "Kart/AIDriverData")]
    public class AIDriverData : ScriptableObject
    {
        public float proximityThreshold = 20.0f; 
        public float updateCornerRange = 50f; 
        public float brakeRange = 80f; 
        public float spinThreshold = 100f; 
        public float speedWhileDrifting = 0.5f;
        public float timeToDrift = 0.5f;
    }
}