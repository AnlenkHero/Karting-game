using System;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Kart
{
    public class LobbyList : MonoBehaviour
    {
        [SerializeField] LobbyListElement lobbyListElementPrefab;
        [SerializeField] private Transform lobbyListElementParent;

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