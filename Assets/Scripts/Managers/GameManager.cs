using System.Collections.Generic;
using Fusion;
using Kart.Controls;
using Kart.Fusion;
using Kart.ModeStrategy;
using UnityEngine;
using Kart.TrackPackage;

namespace Kart
{
    public class GameManager : NetworkBehaviour
    {
        [Networked] public GameModeStrategyFactory strategyFactory { get; set; }

        public PointsTable PointsTable = new();
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

        [Rpc]
        public void RPC_PrepareForRace()
        {
            CurrentGameState = GameState.PreGame;
            PointsTable.CheckAndAddNewPlayers(RoomPlayer.Players);

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

            Strategy.RpcOnStandingUpdate();

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
            Debug.Log("Game Ended with standings.");
            Strategy.RpcOnRaceFinished();
            CurrentGameState = GameState.Finished;
        }
    }
}