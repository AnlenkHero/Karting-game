using UnityEngine;

namespace Kart.TrackPackage
{
    public class LapCheckpoint : MonoBehaviour
    {
        public int index = -1;
        private void OnValidate()
        {
            AutoAssignIndex();
        }

        private void AutoAssignIndex()
        {
            var allCheckpoints = FindObjectsByType<LapCheckpoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            index = allCheckpoints.Length - 1; 
        }
        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out KartController kart)) {
                switch (GameManager.Instance.Strategy)
                {
                    case ICheckpointGameModeStrategy lapsGameModeStrategy:
                        lapsGameModeStrategy.OnPlayerCrossCheckpoint(kart, this);
                        break;
                }
            }
        }
    }
}