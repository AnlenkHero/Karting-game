using System.Collections.Generic;
using Kart.Controls;
using Kart.ModeStrategy;
using UnityEngine;
using Kart.TrackPackage;
using TMPro;

namespace Kart
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameType currentGameType;
        public Track currentTrack;
        public List<KartController> Players = new();
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
            DontDestroyOnLoad(gameObject);
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

            Strategy = IGameModeStrategy.GetGameMode(currentGameType);
            Strategy.InitializeMode();
            CurrentGameState = GameState.Running;
        }

        private void Update()
        {
            if (CurrentGameState is GameState.Finished or GameState.PreGame)
            {
                return;
            }

            ElapsedTime += Time.deltaTime;

            Strategy.UpdateModeLogic();

            ShowCurrentStandings();

            if (Strategy.IsGameOver())
            {
                var standings = Strategy.GetStandings();
                EndGameWithStandings(standings);
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

        public void EndGameWithStandings(List<StandingsEntry> standings)
        {
            Debug.Log("Game Ended! Final Standings:");
            
            foreach (var entry in standings)
            {
                Debug.Log($"{entry.rank}. {entry.player.name} - {entry.additionalInfo["Status"]}");
            }

            CurrentGameState = GameState.Finished;
        }

        public void ShowCurrentStandings()
        {
            var standings = Strategy.GetStandings();
            Debug.Log("Current Standings:");
            text.text = "";
            foreach (var entry in standings)
            {
                Debug.Log($"{entry.rank}. {entry.player.name} - {entry.additionalInfo["LastLapTime"]}");
                text.text += $"{entry.rank}. {entry.player.name} - {entry.additionalInfo["LastLapTime"]}\n";
            }
        }
    }
}