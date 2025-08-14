using Kart.Project_Files.Scripts.Controls;
using UnityEngine;

namespace Kart.Project_Files.Scripts.TrackPackage
{
    [RequireComponent(typeof(Collider))]
    public class ResetCheckpoint : MonoBehaviour
    {
        public int index = -1;
        private void Reset()
        {
            Collider colldr = GetComponent<Collider>();
            if (!colldr.isTrigger)
            {
                colldr.isTrigger = true;
                Debug.Log($"Collider on {gameObject.name} was set to Trigger.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out KartResetter kart))
            {
                kart.currentResetIdx = index;
            }
        }
        
    }
}