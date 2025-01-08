using UnityEngine;
using UnityEngine.Audio;

namespace Kart.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource musicSource;

        public AudioMixer masterMixer;
        public AudioMixerGroup sfxMixer;
        public AudioMixerGroup uiMixer;
        public AudioMixerGroup musicMixer;

        public static readonly string mainVolumeParam = "Volume";
        public static readonly string sfxVolumeParam = "SFXVol";
        public static readonly string uiVolumeParam = "UIVol";
        public static readonly string musicVolumeParam = "MusicVol";
        
    }
}