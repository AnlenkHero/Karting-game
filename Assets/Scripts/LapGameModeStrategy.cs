using System.Linq;
using Kart;

public class LapsGameModeStrategy : IGameModeStrategy
{
    private GameType gameType;
    private int requiredLaps;

    public void InitializeMode(GameType gameType)
    {
        this.gameType = gameType;
        // Extract mode-specific data from gameType
        requiredLaps = gameType.totalLapsRequired;
    }

    public bool CheckForWinCondition(out KartController winner)
    {
        // Example: Check if any player has completed required laps
        // Assuming you have a list of players somewhere in the GameManager:
        foreach (var player in GameManager.Instance.Players)
        {
            if (player.GetComponent<KartLapController>().currentLap >= requiredLaps)
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
    
    public void ProcessCheckpoint(KartLapController kart, LapCheckpoint checkpoint)
    {
        kart.ProcessCheckpoint(checkpoint);
    }
}