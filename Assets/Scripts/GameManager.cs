using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kart
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameType currentGameType;
        public List<KartController> Players = new List<KartController>();
        public float ElapsedTime { get; private set; }

        public IGameModeStrategy Strategy { get; private set; }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            Strategy = CreateStrategyFromGameType(currentGameType);
            Strategy.InitializeMode(currentGameType);
        }

        void Update()
        {
            ElapsedTime += Time.deltaTime;

            // Update the strategy every frame if needed
            Strategy.UpdateModeLogic();

            KartController winner;
            if (Strategy.CheckForWinCondition(out winner))
            {
                EndGame(winner);
            }
            else if (Strategy.IsGameOver())
            {
                HandleNoWinnerScenario();
            }
        }

        private IGameModeStrategy CreateStrategyFromGameType(GameType gameType)
        {
            switch (gameType.modeType)
            {
                case GameModeType.Laps:
                    return new LapsGameModeStrategy();
                default:
                    throw new Exception("Unsupported Game Mode Type");
            }
        }

        private void EndGame(KartController winner)
        {
            Debug.Log("Game Ended! Winner: " + (winner != null ? winner.name : "No winner"));
            // Handle end game UI, network events, etc.
        }

        private void HandleNoWinnerScenario()
        {
            Debug.Log("Game Ended with no winner.");
            // Handle tie or no-winner logic here
        }
    }
}