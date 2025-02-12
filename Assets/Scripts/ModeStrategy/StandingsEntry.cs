using System.Collections.Generic;
using Fusion;
using Kart.Controls;
using Kart.Fusion;

namespace Kart.ModeStrategy
{
    [System.Serializable]
    public struct StandingsEntry
    {
        public int rank;
        public string player;
        public string status;
        public string finishTime;
        public string lapsCompleted;
        public string lastCheckpoint;
        public string lastLapTime;
    }
}