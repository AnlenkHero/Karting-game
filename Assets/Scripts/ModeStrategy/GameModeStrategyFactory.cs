using System;
using Fusion;
using Kart.UI.Strategy;
using UnityEngine;

namespace Kart.ModeStrategy
{
    public class GameModeStrategyFactory : NetworkBehaviour
    {
        [SerializeField] private LapsUiView lapsUiView;
        [SerializeField] private GameEndUiView gameEndUiView;
        public override void Spawned()
        {
            base.Spawned();
            GameManager.Instance.strategyFactory = this;
        }


        public IGameModeStrategy GetGameMode(GameType gameType)
        {
            switch (gameType.modeType)
            {
                case GameModeType.Laps:
                {
                    lapsUiView.gameObject.SetActive(true);
                    return new LapsGameModeStrategy(gameType, lapsUiView, gameEndUiView);
                }
                default:
                    throw new Exception("Unsupported Game Mode Type");
            }
        }
    }
}