using System.Collections.Generic;

namespace Kart.Fusion
{
    public class PointsTable
    {
        // Static dictionary mapping a RoomPlayer to their cumulative points.
        private  readonly Dictionary<RoomPlayer, float> playerPoints = new Dictionary<RoomPlayer, float>();

        // Provides read-only access to the points.
        public  IReadOnlyDictionary<RoomPlayer, float> PlayerPoints => playerPoints;

        // Adds (or increments) points for a given RoomPlayer.
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

        // Gets the current points for a given RoomPlayer.
        public  float GetPoints(RoomPlayer player)
        {
            if (player == null)
                return 0f;

            return playerPoints.TryGetValue(player, out float points) ? points : 0f;
        }

        // Checks the list of current players and adds any new ones with 0 points.
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

        // Returns the RoomPlayer with the highest cumulative points.
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