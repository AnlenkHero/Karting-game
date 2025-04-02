using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.TrackPackage;

namespace Kart.Project_Files.Scripts.ModeStrategy
{
    public interface ICheckpointGameModeStrategy : IGameModeStrategy
    {
        void OnPlayerCrossCheckpoint(KartController kart, LapCheckpoint checkpoint);
        void OnPlayerCrossFinishLine(KartController kart, FinishLine finishLine);
    }

}