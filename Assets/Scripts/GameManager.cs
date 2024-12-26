using System.Collections.Generic;
using UnityEngine;
using Kart.TrackPackage;

namespace Kart
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameType currentGameType;
        public Track currentTrack;
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

        private void Start()
        {
            // Initialize the track (which spawns checkpoints & finish line from its TrackData)
            if (currentTrack != null)
            {
                currentTrack.Initialize();
            }
            else
            {
                Debug.LogWarning("No Track assigned to the GameManager.");
            }

            // Set up the strategy
            Strategy = IGameModeStrategy.GetGameMode(currentGameType);
            Strategy.InitializeMode(currentGameType);
        }

        private void Update()
        {
            ElapsedTime += Time.deltaTime;

            // Update the strategy every frame if needed
            Strategy.UpdateModeLogic();

            // If your design says the game ends as soon as Strategy says "game over"...
            if (Strategy.IsGameOver())
            {
                // We can directly get standings:
                var standings = Strategy.GetFinalStandings();
                EndGameWithStandings(standings);
            }
        }

        public void EndGame(KartController winner)
        {
            Debug.Log("Game Ended! Winner: " + (winner != null ? winner.name : "No winner"));
            // handle end game logic, UI, etc.
        }

        private void HandleNoWinnerScenario()
        {
            Debug.Log("Game Ended with no winner.");
            // handle tie scenario
        }

        public void EndGameWithStandings(List<StandingsEntry> standings)
        {
            Debug.Log("Game Ended! Final Standings:");
            foreach (var entry in standings)
            {
                // "Rank. PlayerName - Info"
                Debug.Log($"{entry.Rank}. {entry.Player.name} - {entry.Info}");
            }

            // Or build a UI panel with this data, etc.
        }
    }
}