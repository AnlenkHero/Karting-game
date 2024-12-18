using Kart.TrackPackage;

namespace Kart
{
    public interface ICheckpointGameModeStrategy : IGameModeStrategy
    {
        void OnPlayerCrossCheckpoint(KartController kart, LapCheckpoint checkpoint);
        void OnPlayerCrossFinishLine(KartController kart, FinishLine finishLine);
    }

}