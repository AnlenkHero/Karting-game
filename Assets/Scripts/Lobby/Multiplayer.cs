using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using Random = UnityEngine.Random;

namespace Kart
{
    [Serializable]
    public enum EncryptionType
    {
        DTLS,
        UDP
    }

    public class Multiplayer : MonoBehaviour
    {
        [SerializeField] string lobbyName = "Lobby";
        [SerializeField] int maxPlayers = 4;
        [SerializeField] EncryptionType encryption = EncryptionType.DTLS;

        public static Multiplayer Instance { get; private set; }

        public string PlayerId { get; private set; }
        public string PlayerName { get; private set; }

        Lobby currentLobby;
        string connectionType => encryption == EncryptionType.DTLS ? k_dtlsEncryption : k_udpEncryption;

        const float k_lobbyHeartbeatInterval = 20f;
        const float k_lobbyPollInterval = 20f;
        const string k_keyJoinCode = "RelayJoinCode";
        const string k_dtlsEncryption = "dtls";
        const string k_udpEncryption = "udp";

        CountdownTimer heartbeatTimer = new CountdownTimer(k_lobbyHeartbeatInterval);
        CountdownTimer pollForUpdatesTimer = new CountdownTimer(k_lobbyPollInterval);
        public static Action OnAuthenticated;
        private List<Player> players;

        async void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);

            await Authenticate();

            heartbeatTimer.OnTimerStop += () =>
            {
                HandleHeartbeatAsync();
                heartbeatTimer.Start();
            };

            pollForUpdatesTimer.OnTimerStop += () =>
            {
                HandlePollForUpdatesAsync();
                pollForUpdatesTimer.Start();
            };
        }

        private void Update()
        {
            heartbeatTimer.Tick(Time.deltaTime);
            pollForUpdatesTimer.Tick(Time.deltaTime);
        }


        async Task Authenticate()
        {
            await Authenticate("Player" + Random.Range(0, 1000));
        }

        async Task Authenticate(string playerName)
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                InitializationOptions options = new InitializationOptions();
                options.SetProfile(playerName);

                await UnityServices.InitializeAsync(options);
            }

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
            };

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                PlayerId = AuthenticationService.Instance.PlayerId;
                PlayerName = playerName;
            }

            OnAuthenticated?.Invoke();
        }

        public async Task<QueryResponse> ListAllLobbies()
        {
            try
            {
                QueryLobbiesOptions options = new QueryLobbiesOptions();
                QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
                return response;
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to list lobbies: " + e.Message);
                return new QueryResponse();
            }
        }

        public async Task CreateLobby()
        {
            try
            {
                Allocation allocation = await AllocateRelay();
                string relayJoinCode = await GetRelayJoinCode(allocation);

                CreateLobbyOptions options = new CreateLobbyOptions
                {
                    IsPrivate = false
                };

                currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
                Debug.Log("Created lobby: " + currentLobby.Name + " with code " + currentLobby.LobbyCode);

                heartbeatTimer.Start();
                pollForUpdatesTimer.Start();

                await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { k_keyJoinCode, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                    }
                });

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                    allocation, connectionType));

                NetworkManager.Singleton.StartHost();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to create lobby: " + e.Message);
            }
        }

        public async Task QuickJoinLobby()
        {
            try
            {
                currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
                pollForUpdatesTimer.Start();

                string relayJoinCode = currentLobby.Data[k_keyJoinCode].Value;
                JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                    joinAllocation, connectionType));

                NetworkManager.Singleton.StartClient();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to quick join lobby: " + e.Message);
            }
        }

        public async Task JoinLobby(string lobbyId)
        {
            try
            {
                currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
                pollForUpdatesTimer.Start();

                string relayJoinCode = currentLobby.Data[k_keyJoinCode].Value;
                JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(
                    joinAllocation, connectionType));

                NetworkManager.Singleton.StartClient();
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to quick join lobby: " + e.Message);
            }
        }

        async Task<Allocation> AllocateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                return allocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError("Failed to allocate relay: " + e.Message);
                return default;
            }
        }

        async Task<string> GetRelayJoinCode(Allocation allocation)
        {
            try
            {
                string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                return relayJoinCode;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError("Failed to get relay join code: " + e.Message);
                return default;
            }
        }

        async Task<JoinAllocation> JoinRelay(string relayJoinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                return joinAllocation;
            }
            catch (RelayServiceException e)
            {
                Debug.LogError("Failed to join relay: " + e.Message);
                return default;
            }
        }

        async Task HandleHeartbeatAsync()
        {
            try
            {
                await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                Debug.Log("Sent heartbeat ping to lobby: " + currentLobby.Name);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to heartbeat lobby: " + e.Message);
            }
        }

        async Task HandlePollForUpdatesAsync()
        {
            try
            {
                if (currentLobby != null)
                {
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                    Debug.Log("Polled for updates on lobby: " + lobby.Name);
                    players = lobby.Players;
                    Debug.Log(players.Count);
                    await LobbyService.Instance.RemovePlayerAsync(lobby.Id, lobby.Players[1].Id);
                }
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError("Failed to poll for updates on lobby: " + e.Message);
            }
        }
        
    }
}