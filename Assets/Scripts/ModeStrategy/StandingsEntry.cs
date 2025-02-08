using System.Collections.Generic;
using Fusion;
using Kart.Controls;

namespace Kart.ModeStrategy
{
    public class StandingsEntry : INetworkStruct //TODO remove dictionary and move to standing specific fields
    {
        public KartController player;
        public int rank;
        public Dictionary<string, string> additionalInfo = new Dictionary<string, string>();
    }
}