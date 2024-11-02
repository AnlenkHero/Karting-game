using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Kart
{
    public class LobbyList : MonoBehaviour
    {
        [SerializeField] private LobbyListElement lobbyListElementPrefab;
        [SerializeField] private Transform lobbyListElementParent;
        [SerializeField] private List<LobbyListElement> lobbies;

        public List<LobbyListElement> Lobbies => lobbies;

        private void OnEnable()
        {
            Multiplayer.OnAuthenticated += CreateLobbyList;
        }

        private void OnDisable()
        {
            Multiplayer.OnAuthenticated -= CreateLobbyList;
        }

        private async void CreateLobbyList()
        {
            try
            {
                QueryResponse response = await Multiplayer.Instance.ListAllLobbies();

                if (response != null && response.Results != null && response.Results.Count > 0)
                {
                    foreach (Lobby lobby in response.Results)
                    {
                        LobbyListElement lobbyListElement = Instantiate(lobbyListElementPrefab, lobbyListElementParent);
                        lobbyListElement.SetupLobbyListElement(
                            $"Lobby: {lobby.Name}, ID: {lobby.Id}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}",
                            () => JoinLobbyAsync(lobby.Id));
                        lobbies.Add(lobbyListElement);
                    }
                }
                else
                {
                    Debug.LogWarning("No lobbies were found.");
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Error listing lobbies: " + e.Message);
            }
        }

        public async void RefreshLobbyList()
        {
            await ClearLobbyList();
            CreateLobbyList();
        }

        private async Task ClearLobbyList()
        {
            lobbies.Clear();
            if (lobbyListElementParent.childCount == 0) return;
            foreach (Transform child in lobbyListElementParent)
            {
                Destroy(child.gameObject);
            }

            await Task.Delay(100);
        }

        private async void JoinLobbyAsync(string lobbyId)
        {
            try
            {
                await Multiplayer.Instance.JoinLobby(lobbyId);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to join lobby {lobbyId}: {e.Message}");
            }
        }
    }
}