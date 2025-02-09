using System.Collections.Generic;
using Fusion;
using Kart.Controls;
using Kart.Fusion;

namespace Kart.ModeStrategy
{
    [System.Serializable]
    public struct StandingsEntry : INetworkStruct //TODO remove dictionary and move to standing specific fields
    {
        public NetworkString<_4> player;
        public int rank;
        public NetworkString<_4> status;
        public NetworkString<_4> finishTime;
        public NetworkString<_4> lapsCompleted;
        public NetworkString<_4> lastCheckpoint;
        public NetworkString<_4> lastLapTime;
    }
}