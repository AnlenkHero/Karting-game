using Kart.Helpers;
using Kart.ModeStrategy;
using Kart.Resources;
using UnityEngine;

namespace Kart.Managers
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