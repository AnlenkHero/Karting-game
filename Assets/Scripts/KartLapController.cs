using Kart.TrackPackage;
using UnityEngine;

namespace Kart
{
    public class KartLapController : MonoBehaviour
    {
        public int currentLap = 0;
        [SerializeField] private int currentCheckpoint = -1;

        public void ProcessCheckpoint(LapCheckpoint checkpoint)
        {
            if (checkpoint.index == currentCheckpoint + 1)
            {
                currentCheckpoint = checkpoint.index;
                if (currentCheckpoint == 0)
                {
                    currentLap++;

                }
            }
        }
        
        public void ProcessFinishLine(FinishLine finishLine) 
        {

        }
    }
}