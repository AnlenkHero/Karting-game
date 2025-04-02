using UnityEngine;
using UnityEngine.Rendering;

namespace Kart.Project_Files.Scripts.Definitions
{
    [CreateAssetMenu(fileName = "New Track", menuName = "Scriptable Object/Track Definition")]
    public class TrackDefinition : ScriptableObject
    {
        public string trackName;
        public Sprite trackIcon;
        public int buildIndex;
        public VolumeProfile volumeProfile;
    }
}