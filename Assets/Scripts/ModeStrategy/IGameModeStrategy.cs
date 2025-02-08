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
        void OnStandingUpdate();
        void OnRaceFinished();
    }
}