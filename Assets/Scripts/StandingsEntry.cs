using System.Collections.Generic;

namespace Kart
{
    public class StandingsEntry
    {
        public KartController player;
        public int rank;
        public Dictionary<string, string> additionalInfo = new Dictionary<string, string>();
    }
}