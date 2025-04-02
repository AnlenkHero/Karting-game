using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.ModeStrategy;
using UnityEngine;

namespace Kart.Project_Files.Scripts.TrackPackage
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