using System;
using System.Collections.Generic;
using Kart.Controls;

namespace Kart.ModeStrategy
{
    public interface IGameModeStrategy
    {
        void InitializeMode();
        bool CheckForWinCondition(out KartController winner);
        bool IsGameOver();
        void UpdateModeLogic();
        List<StandingsEntry> GetStandings();
        public string GetStandingsInfo();

        static IGameModeStrategy GetGameMode(GameType gameType)
        {
            switch (gameType.modeType)
            {
                case GameModeType.Laps:
                    return new LapsGameModeStrategy(gameType);
                default:
                    throw new Exception("Unsupported Game Mode Type");
            }
        }
    }
}