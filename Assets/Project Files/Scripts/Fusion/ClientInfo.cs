using UnityEngine;

namespace Kart.Project_Files.Scripts.Fusion
{
    public static class ClientInfo {
        public static string Username {
            get => PlayerPrefs.GetString("C_Username", "Player");
            set => PlayerPrefs.SetString("C_Username", value);
        }

        public static int KartId {
            get => PlayerPrefs.GetInt("C_KartId", 0);
            set => PlayerPrefs.SetInt("C_KartId", value);
        }

        public static string LobbyName {
            get => PlayerPrefs.GetString("C_LastLobbyName", "");
            set => PlayerPrefs.SetString("C_LastLobbyName", value);
        }
        
        public static string CountryCode {
            get => PlayerPrefs.GetString("C_CountryCode", "np");
            set => PlayerPrefs.SetString("C_CountryCode", value);
        }

        public static bool CountryPrivacy
        {
            get => PlayerPrefs.GetInt("C_CountryPrivacy", 0) == 1;
            set => PlayerPrefs.SetInt("C_CountryPrivacy", value ? 1 : 0);
        }
    }
}