using System.Collections.Generic;
using System.Linq;
using Kart;
using Kart.TrackPackage;
using UnityEngine;

public class LapsGameModeStrategy : ICheckpointGameModeStrategy
{
    private GameType gameType;
    private int requiredLaps;

    private Dictionary<KartController, PlayerLapData> playerLapData = new Dictionary<KartController, PlayerLapData>();
    private int finishedCount;
    private bool halfFinishTriggered;
    private float halfFinishDeadline;
    private int halfPlayersCount;
    private float halfPlayersFinishedTimer = 60f;

    public void InitializeMode(GameType gameType)
    {
        this.gameType = gameType;
        requiredLaps = gameType.totalLapsRequired;

        var allPlayers = GameManager.Instance.Players;
        finishedCount = 0;
        halfFinishTriggered = false;

        halfPlayersCount = Mathf.CeilToInt(allPlayers.Count * 0.5f);

        foreach (var kart in allPlayers)
        {
            playerLapData[kart] = new PlayerLapData
            {
                currentLap = 0,
                currentCheckpoint = 0,
                hasFinished = false,
                finishTime = 0f,
                lapStartTime = GameManager.Instance.ElapsedTime,
                lastLapTime = 0f
            };
        }
    }

    public bool CheckForWinCondition(out KartController winner)
    {
        winner = null;
        return false;
    }

    public bool IsGameOver()
    {
        if (halfFinishTriggered && GameManager.Instance.ElapsedTime >= halfFinishDeadline)
            return true;

        if (finishedCount >= GameManager.Instance.Players.Count)
            return true;

        return false;
    }

    public void UpdateModeLogic()
    {
    }

    public void OnPlayerCrossCheckpoint(KartController kart, LapCheckpoint checkpoint)
    {
        if (!playerLapData.TryGetValue(kart, out var data))
            return;

        int expectedNextCheckpoint = data.currentCheckpoint + 1;

        if (checkpoint.index == expectedNextCheckpoint)
        {
            data.currentCheckpoint = checkpoint.index;
            Debug.Log($"{kart.name} crossed checkpoint {checkpoint.index}.");
        }
        else
        {
            Debug.Log(
                $"{kart.name} hit checkpoint {checkpoint.index} out of order (expected {expectedNextCheckpoint}).");
        }
    }

    public void OnPlayerCrossFinishLine(KartController kart, FinishLine finishLine)
    {
        if (!playerLapData.TryGetValue(kart, out var data) || data.hasFinished) return;

        int totalCheckpoints = GameManager.Instance.currentTrack.checkpoints.Length;
        if (data.currentCheckpoint == (totalCheckpoints - 1))
        {
            data.currentLap++;
            data.currentCheckpoint = -1;
            data.lastLapTime = GameManager.Instance.ElapsedTime - data.lapStartTime;
            data.lapStartTime = GameManager.Instance.ElapsedTime;

            Debug.Log($"{kart.name} completed lap {data.currentLap}/{requiredLaps} " +
                      $"in {data.lastLapTime:F2} seconds.");

            if (data.currentLap >= requiredLaps && !data.hasFinished)
            {
                data.hasFinished = true;
                data.finishTime = GameManager.Instance.ElapsedTime;
                finishedCount++;

                Debug.Log(
                    $"{kart.name} FINISHED! Finish time: {data.finishTime:F2}s (Finished Count = {finishedCount})");

                if (!halfFinishTriggered && finishedCount >= halfPlayersCount)
                {
                    halfFinishTriggered = true;
                    halfFinishDeadline = GameManager.Instance.ElapsedTime + halfPlayersFinishedTimer;
                    Debug.Log(
                        $"Half of the players finished ({finishedCount}/{GameManager.Instance.Players.Count}). Starting {halfPlayersFinishedTimer}s countdown...");
                }

                if (finishedCount >= GameManager.Instance.Players.Count)
                {
                    var standings = GetStandings();
                    GameManager.Instance.EndGameWithStandings(standings);
                }
            }
        }
        else
        {
            Debug.Log($"{kart.name} crossed finish line out of order.");
        }
    }

    public List<StandingsEntry> GetStandings()
    {
        var sortedResults = playerLapData.ToList();
        sortedResults.Sort((a, b) =>
        {
            var dataA = a.Value;
            var dataB = b.Value;

            if (dataA.hasFinished && dataB.hasFinished)
                return dataA.finishTime.CompareTo(dataB.finishTime);

            if (dataA.hasFinished && !dataB.hasFinished)
                return -1;

            if (!dataA.hasFinished && dataB.hasFinished)
                return 1;

            int lapCompare = dataB.currentLap.CompareTo(dataA.currentLap);
            if (lapCompare != 0) return lapCompare;

            return dataB.currentCheckpoint.CompareTo(dataA.currentCheckpoint);
        });

        return sortedResults.Select((kvp, index) =>
        {
            var player = kvp.Key;
            var data = kvp.Value;

            var entry = new StandingsEntry
            {
                player = player,
                rank = index + 1,
                additionalInfo = new Dictionary<string, string>
                {
                    { "Status", data.hasFinished ? "Finished" : "DNF" },
                    { "FinishTime", data.hasFinished ? $"{data.finishTime:F2}s" : "-" },
                    { "LapsCompleted", $"{data.currentLap}/{requiredLaps}" },
                    { "LastCheckpoint", $"Checkpoint {data.currentCheckpoint}" },
                    { "LastLapTime", data.currentLap > 0
                        ? $"{data.lastLapTime:F2}s"
                        : "N/A" }
                }
            };

            return entry;
        }).ToList();
    }

    public PlayerLapData GetPlayerData(KartController kart)
    {
        return playerLapData.TryGetValue(kart, out var data) ? data : null;
    }
}