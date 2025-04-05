using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Kart.Project_Files.Scripts.OtherNetworking
{
    [Serializable]
    public class IpApiResponse {
        public string country;
        public string countryCode;
    }

    public  static class CountryFlagLoader {
        private const string GeoUrl = "http://ip-api.com/json/";
        private const string FlagUrlTemplate = "https://flagcdn.com/w320/{0}.png";
        private static readonly Dictionary<string, Texture2D> FlagCache = new ();
        public static void LoadCountryCode(MonoBehaviour caller, Action<string> callback)
        {
            caller.StartCoroutine(GetCountryCodeRoutine(callback));
        }
        
        public static void LoadFlag(MonoBehaviour caller, string countryCode, Action<Texture2D> callback)
        {
            string lowerCountryCode = countryCode.ToLower();
            
            if (FlagCache.TryGetValue(lowerCountryCode, out Texture2D cachedTexture))
            {
                Debug.Log("Flag loaded from cache for country: " + lowerCountryCode);
                callback?.Invoke(cachedTexture);
                return;
            }
            
            string flagUrl = string.Format(FlagUrlTemplate, lowerCountryCode);
            caller.StartCoroutine(DownloadFlag(flagUrl, lowerCountryCode, callback));
        }

        private static IEnumerator DownloadFlag(string url, string countryCode, Action<Texture2D> callback)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading flag: " + request.error);
                callback?.Invoke(null);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                FlagCache[countryCode] = texture;
                callback?.Invoke(texture);
            }
        }

        private static IEnumerator GetCountryCodeRoutine(Action<string> callback)
        {
            using UnityWebRequest geoRequest = UnityWebRequest.Get(GeoUrl);
            yield return geoRequest.SendWebRequest();

            if (geoRequest.result == UnityWebRequest.Result.ConnectionError ||
                geoRequest.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError("Error fetching geolocation: " + geoRequest.error);
                callback?.Invoke(null);
                yield break;
            }
            
            IpApiResponse response = JsonUtility.FromJson<IpApiResponse>(geoRequest.downloadHandler.text);
            if (string.IsNullOrEmpty(response.countryCode)) {
                Debug.LogError("Country code not found in the geolocation response.");
                callback?.Invoke(null);
                yield break;
            }
            
            string countryCode = response.countryCode.ToLower();
            callback?.Invoke(countryCode);
        }
    }
}