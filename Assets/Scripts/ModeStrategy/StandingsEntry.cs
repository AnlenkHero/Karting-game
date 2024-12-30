using System.Collections.Generic;
using Kart.Controls;

namespace Kart.ModeStrategy
{
    public class StandingsEntry
    {
        public KartController player;
        public int rank;
        public Dictionary<string, string> additionalInfo = new Dictionary<string, string>();
    }
}