using System.Collections.Generic;
using System.Linq;
using Kart;
using Kart.TrackPackage;
using UnityEngine;

public class LapsGameModeStrategy : ICheckpointGameModeStrategy
{
    private GameType gameType;
    private int requiredLaps;

    // Player states
    private Dictionary<KartController, PlayerLapData> playerLapData = new Dictionary<KartController, PlayerLapData>();

    // Tracks how many players have finished
    private int finishedCount;

    // Once half the players finish, we trigger a 1-minute countdown
    private bool halfFinishTriggered;
    private float halfFinishDeadline; // The timestamp (GameManager.Instance.ElapsedTime) when 1 min is up

    // Optionally, store how many players must finish to trigger the countdown
    private int halfPlayersCount;

    public void InitializeMode(GameType gameType)
    {
        this.gameType = gameType;
        requiredLaps = gameType.totalLapsRequired;

        // Initialize all players
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
                finishTime = 0f
            };
        }
    }

    public bool CheckForWinCondition(out KartController winner)
    {
        // We no longer automatically end the race on the first finisher.
        // Instead, we do the "half finish trigger" logic in OnPlayerCrossFinishLine.
        // This method can remain if you want to check if *everyone* finished, or if there's another condition.
        winner = null;
        return false;
    }

    public bool IsGameOver()
    {
        // The race ends if the countdown has started AND the current time is past halfFinishDeadline,
        // or if everyone has finished (optional).
        if (halfFinishTriggered && GameManager.Instance.ElapsedTime >= halfFinishDeadline)
            return true;

        // Optionally, if 100% finished, we can also end the race.
        if (finishedCount >= GameManager.Instance.Players.Count)
            return true;

        return false;
    }

    public void UpdateModeLogic()
    {
        // Check if the countdown has expired
        if (IsGameOver())
        {
            // Produce final standings if not already done
            var standings = GetFinalStandings();
            GameManager.Instance.EndGameWithStandings(standings);
        }
    }

    public void OnPlayerCrossCheckpoint(KartController kart, LapCheckpoint checkpoint)
    {
        if (!playerLapData.ContainsKey(kart))
            return;

        var data = playerLapData[kart];
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
        if (!playerLapData.ContainsKey(kart))
            return;

        var data = playerLapData[kart];
        if (data.hasFinished) return; // Already finished, no need to process again.

        // Check if the player passed all checkpoints
        int totalCheckpoints = GameManager.Instance.currentTrack.checkpoints.Length;
        if (data.currentCheckpoint == (totalCheckpoints - 1))
        {
            data.currentLap++;
            data.currentCheckpoint = -1; // Reset for next lap
            Debug.Log($"{kart.name} completed lap {data.currentLap} / {requiredLaps}");

            // Check if the player has now finished the race
            if (data.currentLap >= requiredLaps && !data.hasFinished)
            {
                data.hasFinished = true;
                data.finishTime = GameManager.Instance.ElapsedTime;
                finishedCount++;

                Debug.Log(
                    $"{kart.name} FINISHED! Finish time: {data.finishTime:F2}s (Finished Count = {finishedCount})");

                // If half the players finished & we haven't triggered the countdown yet, do so
                var totalPlayers = GameManager.Instance.Players.Count;
                if (!halfFinishTriggered && finishedCount >= halfPlayersCount)
                {
                    halfFinishTriggered = true;
                    // We set the deadline to 60 seconds from now
                    halfFinishDeadline = GameManager.Instance.ElapsedTime + 60f;
                    Debug.Log(
                        $"Half of the players finished ({finishedCount}/{totalPlayers}). Starting 60s countdown...");
                }

                // If all players have finished (optional check)
                if (finishedCount >= totalPlayers)
                {
                    // End immediately - everyone is done
                    var standings = GetFinalStandings();
                    GameManager.Instance.EndGameWithStandings(standings);
                }
            }
        }
        else
        {
            Debug.Log($"{kart.name} crossed finish line out of order.");
        }
    }

    public List<StandingsEntry> GetFinalStandings()
    {
        // 1) Sort the dictionary of player lap data
        var sortedResults = SortPlayerDataByStanding(playerLapData);

        // 2) Build a list of StandingsEntry from that sorted data
        var standings = BuildStandings(sortedResults);

        return standings;
    }

    /// <summary>
    /// Sorts players in the correct standing order:
    /// 1) Finished first, by finish time ascending
    /// 2) Otherwise by laps and checkpoints
    /// </summary>
    private List<KeyValuePair<KartController, PlayerLapData>> SortPlayerDataByStanding(
        Dictionary<KartController, PlayerLapData> data)
    {
        var results = data.ToList();
        // results is List<KeyValuePair<KartController, PlayerLapData>>

        results.Sort((a, b) =>
        {
            var dataA = a.Value;
            var dataB = b.Value;

            // If both finished, compare by finish time
            if (dataA.hasFinished && dataB.hasFinished)
            {
                return dataA.finishTime.CompareTo(dataB.finishTime);
            }
            // If only one finished, that one is higher
            else if (dataA.hasFinished && !dataB.hasFinished)
            {
                return -1;
            }
            else if (!dataA.hasFinished && dataB.hasFinished)
            {
                return 1;
            }
            else
            {
                // Neither finished => compare by laps, then checkpoint
                int lapCompare = dataB.currentLap.CompareTo(dataA.currentLap);
                if (lapCompare != 0) return lapCompare;
                return dataB.currentCheckpoint.CompareTo(dataA.currentCheckpoint);
            }
        });

        return results;
    }

    /// <summary>
    /// Builds the final List of StandingsEntry from the sorted KeyValuePairs.
    /// </summary>
    private List<StandingsEntry> BuildStandings(List<KeyValuePair<KartController, PlayerLapData>> sortedResults)
    {
        var standings = new List<StandingsEntry>();

        for (int i = 0; i < sortedResults.Count; i++)
        {
            var kvp = sortedResults[i];
            var player = kvp.Key;
            var data = kvp.Value;

            var infoString = data.hasFinished
                ? $"Finished in {data.finishTime:F2}s"
                : $"DNF (Lap {data.currentLap}, CP {data.currentCheckpoint})";

            standings.Add(new StandingsEntry
            {
                Player = player,
                Rank = i + 1,
                Info = infoString
            });
        }

        return standings;
    }

    public PlayerLapData GetPlayerData(KartController kart)
    {
        if (playerLapData.TryGetValue(kart, out var data))
        {
            return data;
        }

        return null;
    }
}