using UnityEngine;

namespace Kart
{
    public class FinishLine : MonoBehaviour
    {
        private void OnTriggerStay(Collider other) {
            if ( other.TryGetComponent(out KartLapController kart) ) {
                kart.ProcessFinishLine(this);
            }
        }
    }
}