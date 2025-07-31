using System.Collections.Generic;
using System.Linq;

namespace Kart.Project_Files.Scripts.Fusion
{
    public class PointsTable
    {
        private readonly Dictionary<RoomPlayer, float> _playerPoints = new();
        public IReadOnlyDictionary<RoomPlayer, float> PlayerPoints => _playerPoints;

        public float MaxPointsForAllRaces;
        
        public void UpdateMaxPointsForAllRaces(float points)
        {
            MaxPointsForAllRaces += points;
        }
        
        public void AddPoints(RoomPlayer player, float points)
        {
            PruneStalePlayers();

            if (player == null || player.Object == null || !player.Object.IsValid)
                return;

            if (!_playerPoints.TryAdd(player, points))
                _playerPoints[player] += points;
        }

        public int GetPlayerPosition(RoomPlayer player)
        {
            PruneStalePlayers();
            if (player == null) return 0;
            
            var sortedPlayers = GetSortedPlayerPointsList()
                .Select((entry, index) => new { Player = entry.Key, Position = index + 1 })
                .ToList();

            var playerEntry = sortedPlayers.FirstOrDefault(entry => entry.Player == player);
            return playerEntry?.Position ?? 0;
        }

        public float GetPoints(RoomPlayer player)
        {
            PruneStalePlayers();
            if (player == null) return 0f;
            return _playerPoints.TryGetValue(player, out var v) ? v : 0f;
        }
        public void CheckAndAddNewPlayers(IEnumerable<RoomPlayer> currentPlayers)
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
            PruneStalePlayers();
            if (player != null)
                _playerPoints.Remove(player);
        }
        
        public List<KeyValuePair<RoomPlayer,float>> GetSortedPlayerPointsList()
        {
            PruneStalePlayers();
            return _playerPoints
                .OrderByDescending(e => e.Value)
                .ToList();
        }
        
        public RoomPlayer GetWinner()
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
        private void PruneStalePlayers()
        {
            var toRemove = _playerPoints.Keys
                .Where(p => p == null
                            || p.Object == null
                            || !p.Object.IsValid)
                .ToList();

            foreach (var p in toRemove)
                _playerPoints.Remove(p);
        }
    }
}
