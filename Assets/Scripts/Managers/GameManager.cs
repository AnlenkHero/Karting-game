using System.Collections;
using System.Collections.Generic;
using Fusion;
using Kart.Controls;
using Kart.Fusion;
using Kart.Managers;
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
            RPC_DisableKartDriving();
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
            
            RPC_ResetTimer();
        }

        [Rpc]
        private void RPC_StartGame()
        {
            RPC_ResetTimer();
            Strategy = strategyFactory.GetGameMode(currentGameType);
            Strategy.InitializeMode();
            CurrentGameState = GameState.Running;
            RPC_EnableKartDriving();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_ResetTimer()
        {
            ElapsedTime = 0f;
        }
        
        [Rpc]
        private void RPC_DisableKartDriving()
        {
            foreach (var kart in Players)
            {
                kart.canDrive = false;
            }
        }
        
        [Rpc]
        private void RPC_EnableKartDriving()
        {
            foreach (var kart in Players)
            {
                kart.canDrive = true;
            }
        }

        private void Update()
        {
            if (HasStateAuthority && CurrentGameState == GameState.PreGame && ElapsedTime >= 30f)
            {
                RPC_StartGame();
            }

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
            StartCoroutine(WaiForSceneChange(ResourceManager.Instance.tracks[0].buildIndex));
        }

        protected IEnumerator WaiForSceneChange(int sceneId)
        {
            yield return new WaitForSeconds(15f);
            LevelManager.LoadTrack(sceneId);
        }
    }
}