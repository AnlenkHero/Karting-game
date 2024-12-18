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

            if (Strategy.CheckForWinCondition(out KartController winner))
            {
                EndGame(winner);
            }
            else if (Strategy.IsGameOver())
            {
                HandleNoWinnerScenario();
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
    }
}
