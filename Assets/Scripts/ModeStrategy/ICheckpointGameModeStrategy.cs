using Kart.Controls;
using Kart.TrackPackage;

namespace Kart.ModeStrategy
{
    public interface ICheckpointGameModeStrategy : IGameModeStrategy
    {
        void OnPlayerCrossCheckpoint(KartController kart, LapCheckpoint checkpoint);
        void OnPlayerCrossFinishLine(KartController kart, FinishLine finishLine);
    }

}