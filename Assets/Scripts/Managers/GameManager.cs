using System;
using System.Collections.Generic;
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
        public float ElapsedTime { get; private set; }

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
            var standings = Strategy.GetStandings();
            Debug.Log("Game Ended! Final Standings:");
            text.text = string.Empty;

            foreach (var entry in standings)
            {
                Debug.Log(
                    $"{entry.rank}. {entry.player.name} - {entry.additionalInfo["Status"]} - {entry.additionalInfo["LastLapTime"]}");
                text.text +=
                    $"{entry.rank}. {entry.player.name} - {entry.additionalInfo["Status"]} - {entry.additionalInfo["LastLapTime"]}\n";
            }

            CurrentGameState = GameState.Finished;
        }

        public void ShowCurrentStandings()
        {
            var standings = Strategy.GetStandings();
            Debug.Log("Current Standings:");
            text.text = string.Empty;

            foreach (var entry in standings)
            {
                Debug.Log($"{entry.rank}. {entry.player.name} - {entry.additionalInfo["LastLapTime"]}");
                text.text += $"{entry.rank}. {entry.player.name} - {entry.additionalInfo["LastLapTime"]}\n";
            }
        }
    }
}