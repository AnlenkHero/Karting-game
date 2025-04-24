using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.TrackPackage;
using Kart.Project_Files.Scripts.UI.Strategy;
using Kart.Project_Files.Scripts.UI.Strategy.LapsStrategy;
using UnityEngine;

namespace Kart.Project_Files.Scripts.ModeStrategy.LapStrategy
{
    public class LapsGameModeStrategy : ICheckpointGameModeStrategy
    {
        private const float HalfPlayersFinishedTimer = 60f;
        private readonly GameType _gameType;
        private readonly LapsUiView _lapsUiView;
        private readonly GameEndUiView _gameEndUiView;
        private List<PlayerLapData> _playerLapData = new();
        private int _requiredLaps;
        private int _finishedCount;
        private bool _halfFinishTriggered;
        private float _halfFinishDeadline;
        private int _halfPlayersCount;
        public static event Action OnLocalPlayerFinished;

        #region Initializers

        public LapsGameModeStrategy(GameType gameType, LapsUiView lapsUiView, GameEndUiView gameEndUiView)
        {
            _gameType = gameType;
            _lapsUiView = lapsUiView;
            _gameEndUiView = gameEndUiView;
        }

        public void InitializeMode()
        {
            _requiredLaps = _gameType.totalLapsRequired;
            _halfPlayersCount = Mathf.CeilToInt(RoomPlayer.Players.Count * 0.5f);
            InitializePlayerLapData(RoomPlayer.Players);
        }

        private void InitializePlayerLapData(List<RoomPlayer> allPlayers)
        {
            foreach (var roomPlayer in allPlayers)
            {
                var newPlayer = new PlayerLapData
                {
                    player = roomPlayer,
                    lapStartTime = GameManager.Instance.ElapsedTime,
                    lastCheckpointCrossTime = GameManager.Instance.ElapsedTime
                };
                _playerLapData.Add(newPlayer);
            }
        }

        #endregion
        
        #region Core game methods

        public bool IsGameOver()
        {
            if (_halfFinishTriggered && GameManager.Instance.ElapsedTime >= _halfFinishDeadline)
                return true;

            return _finishedCount >= RoomPlayer.Players.Count;
        }


        public void OnRaceFinished()
        {
            var standings = GetStandings().ToList();
            //_lapsUiView.AddOrUpdateStanding(standings);
            var pointsForRace = UpdatePlayersPointTable(standings);

            _lapsUiView.DisableUI();
            _gameEndUiView.ShowEndGameUI(pointsForRace);

            GameManager.Instance.StartCoroutine(DelayScoreboardChange());
        }

        private PointsTable UpdatePlayersPointTable(List<LapStandings> standings)
        {
            PointsTable localPointsForRace = new PointsTable();

            for (int i = 0; i < standings.Count; i++)
            {
                var standing = standings[i];
                if (standing.status != "Finished") continue;

                AddPointsToPointTable(localPointsForRace, standing, i);
                AddPointsToPointTable(GameManager.Instance.PointsTable, standing, i);
            }

            foreach (var rp in RoomPlayer.Players)
            {
                Debug.Log($"{rp.Username}, {GameManager.Instance.PointsTable.GetPoints(rp)}");
            }

            return localPointsForRace;
        }

        public void OnStandingUpdate()
        {
            var standings = GetStandings().ToList();
            _lapsUiView.AddOrUpdateStanding(standings);
        }

        private void AddPointsToPointTable(PointsTable pointsForRace, LapStandings standing, int i)
        {
            pointsForRace.AddPoints(
                RoomPlayer.Players.FirstOrDefault(p => p.Id.ToString() == standing.playerId)!,
                _gameType.pointsForPlacings[i]);
        }

        public void UpdateModeLogic()
        {
            // No continuous logic needed in this scenario
        }

        public bool CheckForWinCondition(out KartController winner)
        {
            winner = null;
            // This particular mode does not identify a single winner mid-race.
            // We rely on OnPlayerCrossFinishLine to finalize finishing logic.
            return false;
        }

        #endregion

        #region Finishline/Checkpoint cross methods

        public void OnPlayerCrossCheckpoint(KartController kart, LapCheckpoint checkpoint)
        {
            var data = _playerLapData.FirstOrDefault(x => x.player.Kart == kart);
            if (data == null)
            {
                Debug.Log("PlayerLapData not found for kart: " + kart.name);
                return;
            }

            int expectedNextCheckpoint = data.currentCheckpoint + 1;

            if (checkpoint.index == expectedNextCheckpoint)
            {
                ProcessCheckpointCorrectCross(kart, checkpoint, data);
            }
            else
            {
                Debug.Log(
                    $"{kart.name} hit checkpoint {checkpoint.index} out of order (expected {expectedNextCheckpoint}).");
            }
        }

        public void OnPlayerCrossFinishLine(KartController kart, FinishLine finishLine)
        {
            var data = _playerLapData.FirstOrDefault(x => x.player.Kart == kart);
            if (data == null || data.hasFinished)
            {
                Debug.Log("PlayerLapData not found for kart: " + kart.name);
                return;
            }


            int totalCheckpoints = GameManager.CurrentTrack.checkpoints.Length;

            if (IsValidFinishLineCross(data, totalCheckpoints))
            {
                CompleteLap(kart, data);

                if (data.currentLap < _requiredLaps || data.hasFinished) return;
                
                EnableSpectatorMode(kart);
                DespawnFinishedPlayer(data);
                MarkFinishedPlayer(kart, data);
                CheckHalfPlayersFinished();
            }
            else
            {
                Debug.Log($"{kart.name} crossed finish line out of order.");
            }
        }

        private static void DespawnFinishedPlayer(PlayerLapData data)
        {
            var kartIndex = RoomPlayer.Players.IndexOf(data.player);
            GameManager.Instance.Runner.Despawn(RoomPlayer.Players[kartIndex].Kart.Object);
        }

        private static void EnableSpectatorMode(KartController kart)
        {
            if (RoomPlayer.Local.Kart == kart)
            {
                Debug.Log("Your kart finished");
                OnLocalPlayerFinished?.Invoke();
            }
        }

        private void CompleteLap(KartController kart, PlayerLapData data)
        {
            data.currentLap++;
            data.currentCheckpoint = -1;

            data.lastLapTime = GameManager.Instance.ElapsedTime - data.lapStartTime;
            data.lapStartTime = GameManager.Instance.ElapsedTime;

            data.lastCheckpointCrossTime = GameManager.Instance.ElapsedTime;

            Debug.Log($"{kart.name} completed lap {data.currentLap}/{_requiredLaps} " +
                      $"in {data.lastLapTime:F2} seconds.");
        }

        private void CheckHalfPlayersFinished()
        {
            if (_halfFinishTriggered || _finishedCount < _halfPlayersCount) return;

            _halfFinishTriggered = true;
            _halfFinishDeadline = GameManager.Instance.ElapsedTime + HalfPlayersFinishedTimer;
            Debug.Log(
                $"Half of the players finished ({_finishedCount}/{RoomPlayer.Players.Count}). " +
                $"Starting {HalfPlayersFinishedTimer}s countdown...");
        }

        private void MarkFinishedPlayer(KartController kart, PlayerLapData data)
        {
            data.hasFinished = true;
            data.finishTime = GameManager.Instance.ElapsedTime;
            _finishedCount++;

            Debug.Log(
                $"{kart.name} FINISHED! Finish time: {data.finishTime:F2}s (Finished Count = {_finishedCount})");
        }

        private void ProcessCheckpointCorrectCross(KartController kart, LapCheckpoint checkpoint, PlayerLapData data)
        {
            data.currentCheckpoint = checkpoint.index;
            data.lastCheckpointCrossTime = GameManager.Instance.ElapsedTime - data.lapStartTime;

            Debug.Log($"{kart.name} crossed checkpoint {checkpoint.index} at {data.lastCheckpointCrossTime:F2}s.");
        }

        private bool IsValidFinishLineCross(PlayerLapData data, int totalCheckpoints)
        {
            return data.currentCheckpoint == (totalCheckpoints - 1);
        }

        #endregion

        #region UI methods

        private IEnumerator DelayScoreboardChange()
        {
            yield return new WaitForSeconds(3);
            _gameEndUiView.ShowEndGameUI(GameManager.Instance.PointsTable);
        }

        #endregion

        #region Standings building  methods

        private IEnumerable<LapStandings> GetStandings()
        {
            _playerLapData.Sort(ComparePlayerResults);
            foreach (PlayerLapData data in _playerLapData.ToList())
            {
                if (data.player.Object == null)
                {
                    _playerLapData.Remove(data);
                }
            }

            return _playerLapData
                .Select((kvp, i) => BuildStandingsEntry(kvp, i + 1));
        }

        /// <summary>
        /// Comparison for sorting players in the standings.
        /// 1) Has finished (and finish time)
        /// 2) Laps completed
        /// 3) Checkpoints reached
        /// 4) lastCheckpointCrossTime (tie-break for same lap/checkpoint)
        /// </summary>
        private int ComparePlayerResults(PlayerLapData dataA, PlayerLapData dataB)
        {
            switch (dataA.hasFinished)
            {
                case true when dataB.hasFinished:
                    // Both finished => compare finish times
                    return dataA.finishTime.CompareTo(dataB.finishTime);
                case true when !dataB.hasFinished:
                    return -1; // A is finished, B is not
                case false when dataB.hasFinished:
                    return 1; // B is finished, A is not
            }

            int lapCompare = dataB.currentLap.CompareTo(dataA.currentLap);

            if (lapCompare != 0)
            {
                return lapCompare;
            }

            int checkpointCompare = dataB.currentCheckpoint.CompareTo(dataA.currentCheckpoint);

            return checkpointCompare != 0
                ? checkpointCompare
                :
                // Tie-break => compare lastCheckpointCrossTime (lower = crossed earlier = leading)
                dataA.lastCheckpointCrossTime.CompareTo(dataB.lastCheckpointCrossTime);
        }

        private LapStandings BuildStandingsEntry(PlayerLapData data, int rank)
        {
            var player = data.player;

            var entry = new LapStandings
            {
                playerId = player.Id.ToString(),
                playerName = player.Username.Value,
                rank = rank,
                status = data.hasFinished ? "Finished" : "DNF",
                finishTime = data.hasFinished ? $"{data.finishTime:F2}s" : "-",
                lapsCompleted = $"{data.currentLap}/{_requiredLaps}",
                lastCheckpoint = $"Checkpoint {data.currentCheckpoint}",
                lastLapTime = data.currentLap > 0 ? $"{data.lastLapTime:F2}s" : "N/A",
            };

            return entry;
        }

        #endregion
    }
}