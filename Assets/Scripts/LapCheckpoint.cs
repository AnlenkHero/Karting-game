using UnityEngine;

namespace Kart
{
    public class LapCheckpoint : MonoBehaviour
    {
        public int index = -1;
        
        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent(out KartLapController kart)) {
                if(GameManager.Instance.Strategy is LapsGameModeStrategy lapsGameModeStrategy)
                {
                    lapsGameModeStrategy.ProcessCheckpoint(kart, this);
                }
            }
        }
    }
}