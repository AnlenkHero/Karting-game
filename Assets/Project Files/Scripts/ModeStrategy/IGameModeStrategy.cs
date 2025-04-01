using System.Collections.Generic;
using Fusion;
using Kart.Controls;

namespace Kart.ModeStrategy
{
    public interface IGameModeStrategy
    {
        void InitializeMode();
        bool CheckForWinCondition(out KartController winner);
        bool IsGameOver();
        void UpdateModeLogic();
        [Rpc]
        void RpcOnStandingUpdate();
        [Rpc]
        void RpcOnRaceFinished();
    }
}