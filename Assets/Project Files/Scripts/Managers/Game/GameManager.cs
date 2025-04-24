using System.Collections;
using System.Collections.Generic;
using Fusion;
using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.ModeStrategy;
using Kart.Project_Files.Scripts.TrackPackage;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Managers.Game
{
    public class GameManager : NetworkBehaviour
    {
        [Networked] public GameModeStrategyFactory StrategyFactory { get; set; }
        [Networked] public float ElapsedTime { get; private set; }
        [Networked] public RaceTrackListManager TrackListManager { get; set; }
        public static readonly List<KartController> Players = new();
        public static Track CurrentTrack;
        public PointsTable PointsTable = new();
        public static GameManager Instance { get; private set; }

        public GameType currentGameType;
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

        public void Update()
        {
            if (HasStateAuthority && CurrentGameState == GameState.PreGame && ElapsedTime >= 5f)
            {
                RPC_StartGame();
            }

            if (CurrentGameState != GameState.Running)
                return;

            Strategy.UpdateModeLogic();

            Strategy.OnStandingUpdate();

            if (!Strategy.IsGameOver()) return;

            EndGameWithStandings();
        }

        [Rpc]
        public void RPC_PrepareForRace()
        {
            RPC_DisableKartDriving();
            CurrentGameState = GameState.PreGame;
            PointsTable.CheckAndAddNewPlayers(RoomPlayer.Players);
            if (HasStateAuthority)
                RPC_ResetTimer();
        }

        [Rpc]
        private void RPC_StartGame()
        {
            if (CurrentTrack != null)
            {
                CurrentTrack.Initialize();
            }
            else
            {
                Debug.LogWarning("No Track assigned to the GameManager.");
            }

            if (HasStateAuthority)
                RPC_ResetTimer();

            Strategy = StrategyFactory.GetGameMode(currentGameType);
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
            Debug.Log("Race Ended with standings.");
            Strategy.OnRaceFinished();
            CurrentGameState = GameState.Finished;

            if (TrackListManager.CurrentRaceCount >= RaceTrackListManager.MaxRaces)
            {
                RoomPlayer sessionWinner = PointsTable.GetWinner();
                Debug.Log("Session Completed! Global Winner: " +
                          (sessionWinner != null ? sessionWinner.name : "No winner"));

                if (HasStateAuthority)
                    StartCoroutine(WaiForSceneChangeAndLoadSessionResults());
            }
            else
            {
                if (HasStateAuthority)
                    StartCoroutine(WaiForSceneChangeAndStartNextRace());
            }
        }

        private IEnumerator WaiForSceneChangeAndStartNextRace()
        {
            TrackListManager.AdvanceToNextRaceTrack();
            yield return new WaitForSeconds(10f);
            GameLauncherNetworkHandler.Instance.Rpc_SetVolumeProfile(TrackListManager.CurrentTrackIndex);
            LevelManager.LoadTrack(TrackListManager.CurrentTrackDefinition.buildIndex);
        }

        private IEnumerator WaiForSceneChangeAndLoadSessionResults()
        {
            yield return new WaitForSeconds(10f);
            //LevelManager.LoadScene("SessionResults");
            foreach (var rp in RoomPlayer.Players)
            {
                Debug.Log($"{rp.Username}, {Instance.PointsTable.GetPoints(rp)}");
            }

            Debug.Log("Loading Session Results Screen...");
        }
    }
}