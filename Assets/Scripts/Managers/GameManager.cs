using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fusion;
using Kart.Controls;
using Kart.Managers;
using Kart.ModeStrategy;
using UnityEngine;
using Kart.TrackPackage;

namespace Kart
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private GameModeStrategyFactory strategyFactory;
        
        public static GameManager Instance { get; private set; }

        public GameType currentGameType;
        public static Track CurrentTrack;
        public static readonly List<KartController> Players = new();
        [Networked] public float ElapsedTime { get; private set; }
        public IGameModeStrategy Strategy { get; private set; }
        public GameState CurrentGameState { get; private set; }

        
        public override void Spawned()
        {
            Players.Clear();
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (HasStateAuthority)
            {
                ElapsedTime += Runner.DeltaTime;
            }
        }

        private void Start()
        {
            CurrentGameState = GameState.PreGame;

            if (CurrentTrack != null)
            {
                CurrentTrack.Initialize();
            }
            else
            {
                Debug.LogWarning("No Track assigned to the GameManager.");
            }
        }

        [Rpc]
        private void RPC_StartGame()
        {
            Strategy = strategyFactory.GetGameMode(currentGameType);
            Strategy.InitializeMode();
            CurrentGameState = GameState.Running;
        }
        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.F) && HasStateAuthority)
                RPC_StartGame();

            if (CurrentGameState is GameState.Finished or GameState.PreGame)
                return;
            
            Strategy.UpdateModeLogic();

            Strategy.OnStandingUpdate();

            if (!Strategy.IsGameOver()) return;

            EndGameWithStandings();
        }

        public void EndGame(KartController winner)
        {
            Debug.Log("Game Ended! Winner: " + (winner != null ? winner.name : "No winner"));
        }

        private void HandleNoWinnerScenario()
        {
            Debug.Log("Game Ended with no winner.");
        }

        private void EndGameWithStandings()
        {
            Strategy.OnRaceFinished();
            CurrentGameState = GameState.Finished;
        }
    }
}