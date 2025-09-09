using Fusion;
using Kart.Project_Files.Scripts.Managers;
using UnityEngine.Rendering;

namespace Kart.Project_Files.Scripts.Fusion
{
    public class GameLauncherNetworkHandler : NetworkBehaviour
    {
        public static GameLauncherNetworkHandler Instance;
        private Volume _volumeProfile;

        public override void Spawned()
        {
            base.Spawned();
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Init(Volume volumeProfile)
        {
            _volumeProfile = volumeProfile;
        }


        [Rpc]
        public void Rpc_SetVolumeProfile(int trackDefinitionIndex)
        {
            _volumeProfile.profile = ResourceManager.Instance.tracks[trackDefinitionIndex].volumeProfile;
        }
    }
}