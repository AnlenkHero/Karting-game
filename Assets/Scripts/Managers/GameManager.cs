using System;
using System.Collections.Generic;
using System.Text;
using Fusion;
using Kart.Controls;
using Kart.ModeStrategy;
using UnityEngine;
using Kart.TrackPackage;
using TMPro;
using UnityEngine.Serialization;

namespace Kart
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameType currentGameType;
        public Track currentTrack;
        public static readonly List<KartController> Players = new();
        [Networked] public float ElapsedTime { get; private set; }
        public IGameModeStrategy Strategy { get; private set; }
        public GameState CurrentGameState { get; private set; }

        public TextMeshProUGUI text;

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
            if (Runner.IsServer)
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
            Strategy = IGameModeStrategy.GetGameMode(currentGameType);
            Strategy.InitializeMode();
            CurrentGameState = GameState.Running;
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                RPC_StartGame();
            }

            if (CurrentGameState is GameState.Finished or GameState.PreGame)
            {
                return;
            }

            ElapsedTime += Time.deltaTime;

            Strategy.UpdateModeLogic();

            ShowCurrentStandings();

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
            ShowCurrentStandings();
            CurrentGameState = GameState.Finished;
        }

        private void ShowCurrentStandings()
        {
            if (!Runner.IsServer) return;
            Rpc_UpdateStandings(Strategy.GetStandingsInfo());
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void Rpc_UpdateStandings(string standingEntry)
        {
            text.text = standingEntry;
            Debug.Log($"Rpc_UpdateStandings received:\n{standingEntry}");
        }

    }
}