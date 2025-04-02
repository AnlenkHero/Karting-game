using System.Collections.Generic;
using System.Linq;

namespace Kart.Project_Files.Scripts.Fusion
{
    public class PointsTable
    {
        private  readonly Dictionary<RoomPlayer, float> playerPoints = new Dictionary<RoomPlayer, float>();
        public  IReadOnlyDictionary<RoomPlayer, float> PlayerPoints => playerPoints;
        
        public  void AddPoints(RoomPlayer player, float points)
        {
            if (player == null)
                return;

            if (playerPoints.ContainsKey(player))
            {
                playerPoints[player] += points;
            }
            else
            {
                playerPoints.Add(player, points);
            }
        }
        
        public  float GetPoints(RoomPlayer player)
        {
            if (player == null)
                return 0f;

            return playerPoints.TryGetValue(player, out float points) ? points : 0f;
        }
        
        public  void CheckAndAddNewPlayers(IEnumerable<RoomPlayer> currentPlayers)
        {
            foreach (var player in currentPlayers)
            {
                if (player != null && !playerPoints.ContainsKey(player))
                {
                    playerPoints.Add(player, 0f);
                }
            }
        }
        
        public void CheckAndDeletePlayer(RoomPlayer player)
        {
            if (playerPoints.ContainsKey(player))
            {
                playerPoints.Remove(player);
            }
        }
        
        public List<KeyValuePair<RoomPlayer, float>> GetSortedPlayerPointsList()
        {
            return playerPoints
                .OrderByDescending(entry => entry.Value)
                .ToList();
        }
        
        public  RoomPlayer GetWinner()
        {
            RoomPlayer winner = null;
            float highestPoints = float.MinValue;
            foreach (var kvp in playerPoints)
            {
                if (kvp.Value > highestPoints)
                {
                    highestPoints = kvp.Value;
                    winner = kvp.Key;
                }
            }
            return winner;
        }
    }
}