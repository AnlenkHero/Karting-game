namespace Kart.ModeStrategy.LapStrategy
{
    [System.Serializable]
    public struct LapStandings
    {
        public int rank;
        public string playerId;
        public string playerName;
        public string status;
        public string finishTime;
        public string lapsCompleted;
        public string lastCheckpoint;
        public string lastLapTime;
    }
}