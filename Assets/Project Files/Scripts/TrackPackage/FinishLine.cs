using Kart.Controls;
using Kart.ModeStrategy;
using UnityEngine;

namespace Kart.TrackPackage
{
    public class FinishLine : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
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