using UnityEngine;

namespace Kart.TrackPackage
{
    public class FinishLine : MonoBehaviour
    {
        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out KartController kart)) {
                switch (GameManager.Instance.Strategy)
                {
                    case ICheckpointGameModeStrategy lapsGameModeStrategy:
                        lapsGameModeStrategy.OnPlayerCrossFinishLine(kart, this);
                        break;
                }
            }
        }
    }
}