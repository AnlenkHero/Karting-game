using UnityEngine;

namespace Kart.TrackPackage
{
    [RequireComponent(typeof(Collider))]
    public class LapCheckpoint : MonoBehaviour
    {
        public int index = -1;
        private void Reset()
        {
            Collider collider = GetComponent<Collider>();
            if (!collider.isTrigger)
            {
                collider.isTrigger = true;
                Debug.Log($"Collider on {gameObject.name} was set to Trigger.");
            }
        }
        private void OnValidate()
        {
            AutoAssignIndex();
        }

        private void AutoAssignIndex()
        {
            var allCheckpoints = FindObjectsByType<LapCheckpoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            index = allCheckpoints.Length - 1; 
        }
        private void OnTriggerEnter(Collider other)
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