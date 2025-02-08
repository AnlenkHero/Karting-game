using Fusion;
using Kart.UI;
using UnityEngine;

namespace Kart.ModeStrategy
{
    public class GameModeStrategyFactory : NetworkBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private LapsUiView lapsUiView;
        
        public IGameModeStrategy GetGameMode(GameType gameType)
        {
            switch (gameType.modeType)
            {
                case GameModeType.Laps:
                {
                    lapsUiView.gameObject.SetActive(true);
                    return new LapsGameModeStrategy(gameType, lapsUiView);
                }
                default:
                    throw new System.Exception("Unsupported Game Mode Type");
            }
        }
    }
}