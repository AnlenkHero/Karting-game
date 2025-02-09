using Fusion;
using Kart.Fusion;

namespace Kart.ModeStrategy
{
    public class PlayerLapData
    {
        public RoomPlayer player;
        public int currentLap;
        public int currentCheckpoint;
        public bool hasFinished;
        public float finishTime;
        public float lastCheckpointCrossTime;
        public float lapStartTime;
        public float lastLapTime;
    }

}