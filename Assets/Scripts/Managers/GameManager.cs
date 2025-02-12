using System;
using System.Collections.Generic;
using System.Text;
using Fusion;
using Kart.Controls;
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
        public Track currentTrack;
        public static readonly List<KartController> Players = new();
        [Networked] public float ElapsedTime { get; private set; }
        public IGameModeStrategy Strategy { get; private set; }
        public GameState CurrentGameState { get; private set; }

        
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
           // DontDestroyOnLoad(gameObject);
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

            if (currentTrack != null)
            {
                currentTrack.Initialize();
            }
            else
            {
                Debug.LogWarning("No Track assigned to the GameManager.");
            }
        }

        [Rpc]
        public void RPC_StartGame()
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

        public void EndGameWithStandings()
        {
            Strategy.OnRaceFinished();
            CurrentGameState = GameState.Finished;
        }
    }
}