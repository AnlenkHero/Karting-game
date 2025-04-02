using Fusion;
using Kart.Project_Files.Scripts.Controls;

namespace Kart.Project_Files.Scripts.ModeStrategy
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