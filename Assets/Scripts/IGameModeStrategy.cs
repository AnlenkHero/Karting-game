using System;

namespace Kart
{
    public interface IGameModeStrategy
    {
        void InitializeMode(GameType gameType);
        bool CheckForWinCondition(out KartController winner);
        bool IsGameOver();
        void UpdateModeLogic();
        
        
        static IGameModeStrategy GetGameMode(GameType gameType)
        {
            switch (gameType.modeType)
            {
                case GameModeType.Laps:
                    return new LapsGameModeStrategy();
                default:
                    throw new Exception("Unsupported Game Mode Type");
            }
        }
    }
}