using System.Collections.Generic;
using System.Linq;
using Fusion;
using Kart.Controls;
using Kart.Fusion;
using Kart.TrackPackage;
using Kart.UI;
using UnityEngine;

namespace Kart.ModeStrategy
{
    public class LapsGameModeStrategy : ICheckpointGameModeStrategy
    {
        private readonly GameType gameType;
        private int requiredLaps;

        private readonly List<PlayerLapData> playerLapData = new();

        private LapsUiView lapsUiView;
        private int finishedCount;
        private bool halfFinishTriggered;
        private float halfFinishDeadline;
        private int halfPlayersCount;
        private readonly float halfPlayersFinishedTimer = 60f;

        public LapsGameModeStrategy(GameType gameType, LapsUiView lapsUiView)
        {
            this.gameType = gameType;
            this.lapsUiView = lapsUiView;
        }

        public void InitializeMode()
        {
            requiredLaps = gameType.totalLapsRequired;

            var allPlayers = RoomPlayer.Players;

            halfPlayersCount = Mathf.CeilToInt(allPlayers.Count * 0.5f);

            foreach (var roomPlayer in allPlayers)
            {
                var newPlayer = new PlayerLapData
                {
                    player = roomPlayer,
                    lapStartTime = GameManager.Instance.ElapsedTime,
                    lastCheckpointCrossTime = GameManager.Instance.ElapsedTime
                };
                playerLapData.Add(newPlayer);
            }
        }

        public bool CheckForWinCondition(out KartController winner)
        {
            winner = null;
            // This particular mode does not identify a single winner mid-race.
            // We rely on OnPlayerCrossFinishLine to finalize finishing logic.
            return false;
        }

        public bool IsGameOver()
        {
            if (halfFinishTriggered && GameManager.Instance.ElapsedTime >= halfFinishDeadline)
                return true;

            return finishedCount >= GameManager.Players.Count;
        }

        public void UpdateModeLogic()
        {
            // No continuous logic needed in this scenario
        }

        public void OnPlayerCrossCheckpoint(KartController kart, LapCheckpoint checkpoint)
        {
            var data = playerLapData.FirstOrDefault(x => x.player.Kart == kart);
            if (data == null)
                return;

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
            var data = playerLapData.FirstOrDefault(x => x.player.Kart == kart);
            if (data == null || data.hasFinished)
                return;


            int totalCheckpoints = GameManager.Instance.currentTrack.checkpoints.Length;

            if (IsValidFinishLineCross(data, totalCheckpoints))
            {
                CompleteLap(kart, data);

                if (data.currentLap < requiredLaps || data.hasFinished) return;

                MarkFinishedPlayer(kart, data);
                CheckHalfPlayersFinished();

                if (finishedCount < GameManager.Players.Count) return;

                GameManager.Instance.EndGameWithStandings();
            }
            else
            {
                Debug.Log($"{kart.name} crossed finish line out of order.");
            }
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

        private void CheckHalfPlayersFinished()
        {
            if (halfFinishTriggered || finishedCount < halfPlayersCount) return;

            halfFinishTriggered = true;
            halfFinishDeadline = GameManager.Instance.ElapsedTime + halfPlayersFinishedTimer;
            Debug.Log(
                $"Half of the players finished ({finishedCount}/{GameManager.Players.Count}). " +
                $"Starting {halfPlayersFinishedTimer}s countdown...");
        }

        private void MarkFinishedPlayer(KartController kart, PlayerLapData data)
        {
            data.hasFinished = true;
            data.finishTime = GameManager.Instance.ElapsedTime;
            finishedCount++;

            Debug.Log(
                $"{kart.name} FINISHED! Finish time: {data.finishTime:F2}s (Finished Count = {finishedCount})");
        }

        private void CompleteLap(KartController kart, PlayerLapData data)
        {
            data.currentLap++;
            data.currentCheckpoint = -1;

            data.lastLapTime = GameManager.Instance.ElapsedTime - data.lapStartTime;
            data.lapStartTime = GameManager.Instance.ElapsedTime;

            data.lastCheckpointCrossTime = GameManager.Instance.ElapsedTime;

            Debug.Log($"{kart.name} completed lap {data.currentLap}/{requiredLaps} " +
                      $"in {data.lastLapTime:F2} seconds.");
        }

        private IEnumerable<StandingsEntry> GetStandings()
        {
            playerLapData.Sort(ComparePlayerResults);

            return playerLapData
                .Select((kvp, i) => BuildStandingsEntry(kvp, i + 1));
        }

        public void OnStandingUpdate()
        {
            var standings = GetStandings().ToList();
            lapsUiView.AddOrUpdateStanding(standings);
        }

        public void OnRaceFinished()
        {
            var standings = GetStandings().ToList();
            lapsUiView.AddOrUpdateStanding(standings);
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

            //Compare laps
            int lapCompare = dataB.currentLap.CompareTo(dataA.currentLap);

            if (lapCompare != 0)
            {
                return lapCompare;
            }

            //Compare checkpoints
            int checkpointCompare = dataB.currentCheckpoint.CompareTo(dataA.currentCheckpoint);

            return checkpointCompare != 0
                ? checkpointCompare
                :
                // Tie-break => compare lastCheckpointCrossTime (lower = crossed earlier = leading)
                dataA.lastCheckpointCrossTime.CompareTo(dataB.lastCheckpointCrossTime);
        }

        /// <summary>
        /// Builds a single StandingsEntry for sorted player data.
        /// </summary>
        private StandingsEntry BuildStandingsEntry(PlayerLapData data, int rank)
        {
            var player = data.player;

            var entry = new StandingsEntry
            {
                player = player.Id.ToString(),
                rank = rank,
                status = data.hasFinished ? "Finished" : "DNF",
                finishTime = data.hasFinished ? $"{data.finishTime:F2}s" : "-",
                lapsCompleted = $"{data.currentLap}/{requiredLaps}",
                lastCheckpoint = $"Checkpoint {data.currentCheckpoint}",
                lastLapTime = data.currentLap > 0 ? $"{data.lastLapTime:F2}s" : "N/A",
            };

            return entry;
        }
    }
}