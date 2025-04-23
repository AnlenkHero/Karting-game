using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
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
        private readonly GameType gameType;
        private readonly LapsUiView lapsUiView;
        private readonly GameEndUiView gameEndUiView;

        private int requiredLaps;
        private List<PlayerLapData> playerLapData = new();
        private int finishedCount;
        private bool halfFinishTriggered;
        private float halfFinishDeadline;
        private int halfPlayersCount;
        private readonly float halfPlayersFinishedTimer = 60f;
        public static event Action OnLocalPlayerFinished; 
        public LapsGameModeStrategy(GameType gameType, LapsUiView lapsUiView, GameEndUiView gameEndUiView)
        {
            this.gameType = gameType;
            this.lapsUiView = lapsUiView;
            this.gameEndUiView = gameEndUiView;
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

            return finishedCount >= RoomPlayer.Players.Count;
        }

        public void UpdateModeLogic()
        {
            // No continuous logic needed in this scenario
        }

        public void OnPlayerCrossCheckpoint(KartController kart, LapCheckpoint checkpoint)
        {
            var data = playerLapData.FirstOrDefault(x => x.player.Kart == kart);
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
            var data = playerLapData.FirstOrDefault(x => x.player.Kart == kart);
            if (data == null || data.hasFinished)
            {
                Debug.Log("PlayerLapData not found for kart: " + kart.name);
                return;
            }


            int totalCheckpoints = GameManager.CurrentTrack.checkpoints.Length;

            if (IsValidFinishLineCross(data, totalCheckpoints))
            {
                CompleteLap(kart, data);

                if (data.currentLap < requiredLaps || data.hasFinished) return;
                if (RoomPlayer.Local.Kart == kart)
                {
                    Debug.Log("Your kart finished");
                    OnLocalPlayerFinished?.Invoke();
                }
                var kartIndex = RoomPlayer.Players.IndexOf(data.player);
                GameManager.Instance.Runner.Despawn(RoomPlayer.Players[kartIndex].Kart.Object);
                MarkFinishedPlayer(kart, data);
                CheckHalfPlayersFinished();
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
                $"Half of the players finished ({finishedCount}/{RoomPlayer.Players.Count}). " +
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

        private IEnumerable<LapStandings> GetStandings()
        {
            playerLapData.Sort(ComparePlayerResults);
            foreach (PlayerLapData data in playerLapData.ToList())
            {
                if (data.player.Object == null)
                {
                    playerLapData.Remove(data);
                }
            }

            return playerLapData
                .Select((kvp, i) => BuildStandingsEntry(kvp, i + 1));
        }

        [Rpc]
        public void RpcOnStandingUpdate()
        {
            var standings = GetStandings().ToList();
            lapsUiView.AddOrUpdateStanding(standings);
        }

        [Rpc]
        public void RpcOnRaceFinished()
        {
            var standings = GetStandings().ToList();
            lapsUiView.AddOrUpdateStanding(standings);
            PointsTable pointsForRace = new PointsTable();

            for (int i = 0; i < standings.Count; i++)
            {
                var standing = standings[i];
                if (standing.status == "Finished")
                {
                    pointsForRace.AddPoints(
                        RoomPlayer.Players.FirstOrDefault(p => p.Id.ToString() == standing.playerId)!,
                        gameType.pointsForPlacings[i]);

                    GameManager.Instance.PointsTable.AddPoints(
                        RoomPlayer.Players.FirstOrDefault(p => p.Id.ToString() == standing.playerId),
                        gameType.pointsForPlacings[i]);
                }
            }

            lapsUiView.DisableUI();
            gameEndUiView.ShowEndGameUI(GameManager.Instance.PointsTable);

            foreach (var rp in RoomPlayer.Players)
            {
                Debug.Log($"{rp.Username}, {GameManager.Instance.PointsTable.GetPoints(rp)}");
            }

            GameManager.Instance.StartCoroutine(WaitForYou());
        }

        private IEnumerator WaitForYou()
        {
            yield return new WaitForSeconds(3);
            gameEndUiView.ShowEndGameUI(GameManager.Instance.PointsTable);
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
                lapsCompleted = $"{data.currentLap}/{requiredLaps}",
                lastCheckpoint = $"Checkpoint {data.currentCheckpoint}",
                lastLapTime = data.currentLap > 0 ? $"{data.lastLapTime:F2}s" : "N/A",
            };

            return entry;
        }
    }
}