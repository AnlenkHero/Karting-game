using System.Collections.Generic;
using System.Linq;

namespace Kart.Project_Files.Scripts.Fusion
{
    public class PointsTable
    {
        private  readonly Dictionary<RoomPlayer, float> _playerPoints = new ();
        public  IReadOnlyDictionary<RoomPlayer, float> PlayerPoints => _playerPoints;
        
        public  void AddPoints(RoomPlayer player, float points)
        {
            if (player == null)
                return;

            if (!_playerPoints.TryAdd(player, points))
            {
                _playerPoints[player] += points;
            }
        }
        
        public  float GetPoints(RoomPlayer player)
        {
            if (player == null)
                return 0f;

            return _playerPoints.TryGetValue(player, out float points) ? points : 0f;
        }
        
        public  void CheckAndAddNewPlayers(IEnumerable<RoomPlayer> currentPlayers)
        {
            foreach (var player in currentPlayers)
            {
                if (player != null && !_playerPoints.ContainsKey(player))
                {
                    _playerPoints.Add(player, 0f);
                }
            }
        }
        
        public void CheckAndDeletePlayer(RoomPlayer player)
        {
            if (_playerPoints.ContainsKey(player))
            {
                _playerPoints.Remove(player);
            }
        }
        
        public List<KeyValuePair<RoomPlayer, float>> GetSortedPlayerPointsList()
        {
            return _playerPoints
                .OrderByDescending(entry => entry.Value)
                .ToList();
        }
        
        public  RoomPlayer GetWinner()
        {
            RoomPlayer winner = null;
            float highestPoints = float.MinValue;
            foreach (var kvp in _playerPoints.Where(kvp => kvp.Value > highestPoints))
            {
                highestPoints = kvp.Value;
                winner = kvp.Key;
            }
            return winner;
        }
    }
}