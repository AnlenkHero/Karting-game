using System;
using Fusion;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.ModeStrategy.LapStrategy;
using Kart.Project_Files.Scripts.UI.Strategy;
using Kart.Project_Files.Scripts.UI.Strategy.LapsStrategy;
using UnityEngine;

namespace Kart.Project_Files.Scripts.ModeStrategy
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