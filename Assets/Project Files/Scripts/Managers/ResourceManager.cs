using Kart.Project_Files.Scripts.Definitions;
using Kart.Project_Files.Scripts.Helpers;
using Kart.Project_Files.Scripts.ModeStrategy;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Managers
{
    public class ResourceManager : MonoBehaviour
    {
        // public GameUI hudPrefab;
        //public NicknameUI nicknameCanvasPrefab;
        public KartDefinition[] kartDefinitions;
        public GameType[] gameTypes;
        public TrackDefinition[] tracks;
        //public Powerup[] powerups;
        //public Powerup noPowerup;

        public static ResourceManager Instance => Singleton<ResourceManager>.Instance;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}