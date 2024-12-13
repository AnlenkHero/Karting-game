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
                    if (currentLap > totalLaps)
                    {
                    }
                }
            }
        }
        
        public void ProcessFinishLine(FinishLine finishLine) 
        {
            if ( CheckpointIndex == checkpoints.Length - 1 || finishLine.debug ) {
                // If we have just started the race we dont want to complete a lap. This is a small workaround.
                if ( Lap == 0 ) return;
        
                // Add our current tick to the LapTicks networked property so we can keep track of race times.
                LapTicks.Set(Lap - 1, Runner.Tick);

                // Increment the lap and reset the checkpoint index to -1. This tells checkpoint code that we have just
                // touched the finish line.
                Lap++;
                CheckpointIndex = -1;
            }
        }
    }
}