using UnityEngine;

namespace Kart.Project_Files.Scripts.ModeStrategy
{
    [CreateAssetMenu(fileName = "GameType", menuName = "GameModes/New GameType")]
    public class GameType : ScriptableObject
    {
        public string gameModeName;
        public string description;
        public Sprite icon;

        public int totalLapsRequired;
        public int targetPoints;
        public float timeLimit;
        public float matchDuration;
        public int playersToRemain;

        public GameModeType modeType;

        public float[] pointsForPlacings;
    }

    public enum GameModeType
    {
        Laps,
        Points,
        Elimination,
        Timer
    }
}