using UnityEngine;

namespace Kart
{
    public class PlayerProgress : MonoBehaviour
    {
        public int lapCount { get; set; }
        public int checkpointIndex { get; set; }
        public int score { get; set; }
        public float time { get; set; }
    }
}