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
    public void InitializeMode(GameType gameType)
    {
        this.gameType = gameType;
        requiredLaps = gameType.totalLapsRequired;

        // Find the Track in the scene (assuming only one track is active).

        // Initialize lap data for each player
        foreach (var kart in GameManager.Instance.Players)
        {
            playerLapData[kart] = new PlayerLapData
            {
                currentLap = 0,
                currentCheckpoint = 0
            };
        }
    }

    public bool CheckForWinCondition(out KartController winner)
    {
        // Example: Check if any player has completed required laps
        foreach (var kvp in playerLapData)
        {
            KartController player = kvp.Key;
            PlayerLapData lapData = kvp.Value;

            if (lapData.currentLap >= requiredLaps)
            {
                winner = player;
                return true;
            }
        }

        winner = null;
        return false;
    }

    public bool IsGameOver()
    {
        // In a laps scenario, game ends as soon as someone finishes
        KartController dummyWinner;
        return CheckForWinCondition(out dummyWinner);
    }

    public void UpdateModeLogic()
    {
        // Any per-frame logic specific to laps mode could go here
    }
    
    
    /// <summary>
    /// Called by a LapCheckpoint script when a player crosses a normal checkpoint.
    /// </summary>
    public void OnPlayerCrossCheckpoint(KartController kart, LapCheckpoint checkpoint)
    {
        // Safety check: is this a recognized player?
        if (!playerLapData.ContainsKey(kart))
            return;

        var data = playerLapData[kart];
        
        int expectedNextCheckpoint = data.currentCheckpoint + 1;
        
        // If this checkpoint matches the expected index, update the player's progress
        if (checkpoint.index == expectedNextCheckpoint)
        {
            data.currentCheckpoint = checkpoint.index;
            Debug.Log($"{kart.name} crossed checkpoint {checkpoint.index}.");

            // If you want the finish line to be a completely separate object, 
            // no lap increment happens here. That will happen on the finish line trigger.
        }
        // else: The player crossed the wrong checkpoint out of sequence (cheating?), handle if needed
    }
    
    /// <summary>
    /// Called by the FinishLine script when a player crosses the finish line.
    /// </summary>
    public void OnPlayerCrossFinishLine(KartController kart, FinishLine finishLine)
    {
        if (!playerLapData.ContainsKey(kart))
            return;

        var data = playerLapData[kart];

        // A common approach is: crossing the finish line *only counts* if they've passed all checkpoints.
        // For example, if the track has N checkpoints, the lastCheckpointIndex should be (N - 1) 
        // before crossing the finish line increments the lap.
        if (data.currentCheckpoint == (GameManager.Instance.currentTrack.checkpoints.Length - 1))
        {
            data.currentLap++;
            data.currentCheckpoint = -1; // Reset checkpoint index for the next lap
            Debug.Log($"{kart.name} completed lap {data.currentLap} / {requiredLaps}");

            // Optionally: Check for win condition right here
            KartController winner;
            if (CheckForWinCondition(out winner))
            {
                GameManager.Instance.EndGame(winner); 
            }
        }
        else
        {
            // Player attempted to finish a lap without hitting all checkpoints.
            Debug.Log($"{kart.name} crossed finish line out of order.");
        }
    }
}