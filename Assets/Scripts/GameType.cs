using UnityEngine;

[CreateAssetMenu(fileName = "GameType", menuName = "GameModes/New GameType")]
public class GameType : ScriptableObject
{
    public string gameModeName;
    public string description;
    public Sprite icon;

    // Shared or mode-specific fields:
    public int totalLapsRequired;
    public int targetPoints;
    public float timeLimit;
    public float matchDuration;
    public int playersToRemain;

    // Instead of referencing managers directly, we can store an enum or a reference to a strategy class name
    public GameModeType modeType;
}

public enum GameModeType
{
    Laps,
    Points,
    Elimination,
    Timer
}