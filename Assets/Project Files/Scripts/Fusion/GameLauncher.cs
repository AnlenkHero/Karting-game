using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using Kart.Project_Files.Scripts.Helpers;
using Kart.Project_Files.Scripts.Managers;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.Managers.Interface;
using Kart.Project_Files.Scripts.UI.Fusion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Kart.Project_Files.Scripts.Fusion
{
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Failed,
        Connected
    }

    [RequireComponent(typeof(LevelManager))]
    public class GameLauncher : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private GameManager _gameManagerPrefab;
        [SerializeField] private RoomPlayer _roomPlayerPrefab;
        [SerializeField] private DisconnectUI _disconnectUI;
        [SerializeField] private Volume _volumeProfile;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private GameLauncherNetworkHandler _gameLauncherNetworkHandler;
        [SerializeField] private DummySearchingUI _searchingUI;

        public static ConnectionStatus ConnectionStatus = ConnectionStatus.Disconnected;

        private GameMode _gameMode;
        private NetworkRunner _runner;
        private bool _isSearchingMatchMakingSession;
        public FusionObjectPoolRoot _pool;
        private Coroutine _serverStartRoutine;
        private Coroutine _clientStartRoutine;
        public static GameLauncher Instance => Singleton<GameLauncher>.Instance;

        private void Start()
        {
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);
            SceneManager.LoadScene(LevelManager.MAIN_MENU_SCENE);
        }

        #region Session Creation/Join

        private void CreateFusionRunner()
        {
            GameObject go = new GameObject("MAIN Runner Session GO");
            DontDestroyOnLoad(go);

            _runner = go.AddComponent<NetworkRunner>();
            var sim3D = go.AddComponent<RunnerSimulatePhysics3D>();
            sim3D.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;

            _gameMode = GameMode.AutoHostOrClient;
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);
            _pool = go.AddComponent<FusionObjectPoolRoot>();

            Debug.Log($"Created gameobject {go.name} - joining matchmaking lobby");
        }

        public async Task JoinOrCreateMatchmakingLobby()
        {
            PrepareForSearching();

            if (_runner != null)
                LeaveSession();

            CreateFusionRunner();
            await TryToJoinMatchmakingLobby();
        }

        private async Task TryToJoinMatchmakingLobby()
        {
            var joinLobbyResult = await _runner.JoinSessionLobby(SessionLobby.Custom, "MatchmakingLobby");
            if (joinLobbyResult.Ok)
            {
                Debug.Log("Successfully joined lobby: MatchmakingLobby. Awaiting session list update...");
            }
            else
            {
                Debug.LogError("Failed to join lobby: " + joinLobbyResult.ShutdownReason);
                SetConnectionStatus(ConnectionStatus.Failed);
            }
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            Debug.Log("Session list updated. Count: " + sessionList.Count);

            SessionInfo targetSession = FindExistingSession(sessionList);

            if (targetSession != null)
            {
                Debug.Log("Found joinable session: " + targetSession.Name);
                StartMatchmakingGameSession(targetSession.Name, enableCreation: false);
            }
            else
            {
                Debug.Log("No joinable session found. Creating a new session.");
                var newSessionName = GenerateSessionName();
                StartMatchmakingGameSession(newSessionName, enableCreation: true);
            }
        }

        private SessionInfo FindExistingSession(List<SessionInfo> sessionList)
        {
            return sessionList
                .FirstOrDefault(
                    s => s.IsOpen && s.PlayerCount < s.MaxPlayers && s.Name.StartsWith("MatchmakingSession"));
        }

        private async void StartMatchmakingGameSession(string sessionName, bool enableCreation)
        {
            try
            {
                Debug.Log($"Starting game session: {sessionName} (Enable Creation: {enableCreation})");

                var startArgs = ConfigureStartGameArgs(sessionName, enableCreation);

                var result = await _runner.StartGame(startArgs);
                if (result.Ok)
                {
                    Debug.Log("Successfully started game session: " + sessionName);
                }
                else
                {
                    Debug.LogError("Failed to start game session: " + result.ShutdownReason);
                    SetConnectionStatus(ConnectionStatus.Failed);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception in StartGameSession: " + e);
                SetConnectionStatus(ConnectionStatus.Failed);
            }
        }

        private StartGameArgs ConfigureStartGameArgs(string sessionName, bool enableCreation)
        {
            return new StartGameArgs
            {
                GameMode = _gameMode,
                SessionName = sessionName,
                ObjectProvider = _pool,
                SceneManager = _levelManager,
                PlayerCount = 2,
                EnableClientSessionCreation = enableCreation,
                MatchmakingMode = MatchmakingMode.FillRoom
            };
        }

        private void ServerGameStarted()
        {
            _serverStartRoutine = StartCoroutine(WaitForServerGameStart());
        }

        private IEnumerator WaitForServerGameStart()
        {
            _runner.SessionInfo.IsOpen = false;
            Debug.Log("SERVER: All players joined. Starting the game now...");
            yield return new WaitForSeconds(5f);
            if (_runner == null) yield break;
            if (_runner.SessionInfo.PlayerCount != _runner.SessionInfo.MaxPlayers)
            {
                Debug.Log("SERVER: Not all players are present. Restarting session search.");
                _runner.SessionInfo.IsOpen = true;
            }
            else if (_runner)
            {
                Debug.Log("SERVER: All players joined. Starting the game now...");
                _isSearchingMatchMakingSession = false;
                GameManager.Instance.TrackListManager.AdvanceToNextRaceTrack();
                GameLauncherNetworkHandler.Instance.Rpc_SetVolumeProfile(GameManager.Instance.TrackListManager
                    .CurrentTrackIndex);
                LevelManager.LoadTrack(GameManager.Instance.TrackListManager.CurrentTrackDefinition.buildIndex);
            }
        }

        private IEnumerator WaitForClientGameStart()
        {
            ToggleSearchingUIVisibility(false);
            Debug.Log("Client game started. Loading track...");
            yield return new WaitForSeconds(4.5f);
            if (!_isSearchingMatchMakingSession || _runner == null) yield break;
            if (_runner.SessionInfo.PlayerCount != _runner.SessionInfo.MaxPlayers)
            {
                Debug.Log("CLIENT: Not all players are present. Restarting session search.");
                _searchingUI.gameObject.SetActive(true);
                _searchingUI.StartSearching();
            }
            else if (_runner)
            {
                Debug.Log("CLIENT: All players joined. Starting the game now...");
                _isSearchingMatchMakingSession = false;
                GameLauncherNetworkHandler.Instance.Init(_volumeProfile);
                InterfaceManager.Instance.SetRootScreen(null);
            }
        }

        private void ClientGameStarted()
        {
            _clientStartRoutine = StartCoroutine(WaitForClientGameStart());
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("Connected to server");
            SetConnectionStatus(ConnectionStatus.Connected);
        }


        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
            if (runner.TryGetSceneInfo(out var scene) && scene.SceneCount > 0)
            {
                Debug.LogWarning($"Refused connection requested by {request.RemoteAddress}");
                request.Refuse();
            }
            else
                request.Accept();
        }


        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player {player} Joined!");

            if (runner.IsServer)
            {
                if (_gameMode == GameMode.AutoHostOrClient)
                {
                    runner.Spawn(_gameManagerPrefab, Vector3.zero, Quaternion.identity);
                    runner.Spawn(_gameLauncherNetworkHandler, Vector3.zero, Quaternion.identity);
                }

                var roomPlayer = runner.Spawn(_roomPlayerPrefab, Vector3.zero, Quaternion.identity, player);
                roomPlayer.GameState = RoomPlayer.EGameState.Lobby;
            }

            AutoStartGameWhenMaxPlayers(runner);

            SetConnectionStatus(ConnectionStatus.Connected);
        }

        private void AutoStartGameWhenMaxPlayers(NetworkRunner runner)
        {
            if (runner.SessionInfo.PlayerCount != runner.SessionInfo.MaxPlayers) return;

            _isSearchingMatchMakingSession = true;
            ClientGameStarted();
            if (runner.IsServer)
            {
                ServerGameStarted();
            }
        }

        #endregion

        #region Session Leave/Shutdown

        private bool RestartSearchOnEmergencyShutdown()
        {
            if (!_isSearchingMatchMakingSession) return false;

            Debug.Log("Session closed during matchmaking → restart search");
            LeaveSession();
            _ = JoinOrCreateMatchmakingLobby();
            ToggleSearchingUIVisibility(true);
            return true;
        }

        public void LeaveSession()
        {
            Debug.Log("Leaving session...");
            ToggleSearchingUIVisibility(false);
            StopRoutine(ref _serverStartRoutine);
            StopRoutine(ref _clientStartRoutine);
            _isSearchingMatchMakingSession = false;

            if (_runner != null) _runner.Shutdown();
            else SetConnectionStatus(ConnectionStatus.Disconnected);
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            Debug.Log("Disconnected from server");
            LeaveSession();
            SetConnectionStatus(ConnectionStatus.Disconnected);
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.Log($"Connect failed {reason}");
            LeaveSession();
            SetConnectionStatus(ConnectionStatus.Failed);
            (string status, string message) = ConnectFailedReasonToHuman(reason);
            _disconnectUI.ShowMessage(status, message);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"{player.PlayerId} disconnected.");
            var roomPlayer = RoomPlayer.GetPlayer(runner, player);
            GameManager.Instance.PointsTable.CheckAndDeletePlayer(roomPlayer);
            RoomPlayer.RemovePlayer(runner, player);
            SetConnectionStatus(ConnectionStatus);
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (RestartSearchOnEmergencyShutdown()) return;

            Debug.Log($"OnShutdown {shutdownReason}");
            SetConnectionStatus(ConnectionStatus.Disconnected);
            (string status, string message) = ShutdownReasonToHuman(shutdownReason);
            _disconnectUI.ShowMessage(status, message);

            DisposeNetworkedData();
        }

        private void DisposeNetworkedData()
        {
            RoomPlayer.Players.Clear();

            if (_runner)
                Destroy(_runner.gameObject);

            _pool?.ClearPools();
            _pool = null;
            _runner = null;
        }

        private (string, string) ShutdownReasonToHuman(ShutdownReason reason)
        {
            switch (reason)
            {
                case ShutdownReason.Ok:
                    return (null, null);
                case ShutdownReason.Error:
                    return ("Error", "Shutdown was caused by some internal error");
                case ShutdownReason.IncompatibleConfiguration:
                    return ("Incompatible Config", "Mismatching type between client Server Mode and Shared Mode");
                case ShutdownReason.ServerInRoom:
                    return ("Room name in use",
                        "There's a room with that name! Please try a different name or wait a while.");
                case ShutdownReason.DisconnectedByPluginLogic:
                    return ("Disconnected By Plugin Logic", "You were kicked, the room may have been closed");
                case ShutdownReason.GameClosed:
                    return ("Game Closed", "The session cannot be joined, the game is closed");
                case ShutdownReason.GameNotFound:
                    return ("Game Not Found", "This room does not exist");
                case ShutdownReason.MaxCcuReached:
                    return ("Max Players", "The Max CCU has been reached, please try again later");
                case ShutdownReason.InvalidRegion:
                    return ("Invalid Region", "The currently selected region is invalid");
                case ShutdownReason.GameIdAlreadyExists:
                    return ("ID already exists", "A room with this name has already been created");
                case ShutdownReason.GameIsFull:
                    return ("Game is full", "This lobby is full!");
                case ShutdownReason.InvalidAuthentication:
                    return ("Invalid Authentication", "The Authentication values are invalid");
                case ShutdownReason.CustomAuthenticationFailed:
                    return ("Authentication Failed", "Custom authentication has failed");
                case ShutdownReason.AuthenticationTicketExpired:
                    return ("Authentication Expired", "The authentication ticket has expired");
                case ShutdownReason.PhotonCloudTimeout:
                    return ("Cloud Timeout", "Connection with the Photon Cloud has timed out");
                case ShutdownReason.AlreadyRunning:
                    return ("Already Running", "The game is already running");
                case ShutdownReason.InvalidArguments:
                    return ("Invalid Arguments", "The arguments provided are invalid");
                case ShutdownReason.HostMigration:
                    return ("Host Migration", "The host has migrated");
                case ShutdownReason.ConnectionTimeout:
                    return ("Connection Timeout", "The connection has timed out");
                case ShutdownReason.ConnectionRefused:
                    return ("Connection Refused", "The connection was refused");
                case ShutdownReason.OperationTimeout:
                    return ("Operation Timeout", "The operation has timed out");
                case ShutdownReason.OperationCanceled:
                    return ("Operation Canceled", "The operation has been canceled");
                default:
                    Debug.LogWarning($"Unknown ShutdownReason {reason}");
                    return ("Unknown Shutdown Reason", $"{(int)reason}");
            }
        }

        private (string, string) ConnectFailedReasonToHuman(NetConnectFailedReason reason)
        {
            switch (reason)
            {
                case NetConnectFailedReason.Timeout:
                    return ("Timed Out", "");
                case NetConnectFailedReason.ServerRefused:
                    return ("Connection Refused", "The lobby may be currently in-game");
                case NetConnectFailedReason.ServerFull:
                    return ("Server Full", "");
                default:
                    Debug.LogWarning($"Unknown NetConnectFailedReason {reason}");
                    return ("Unknown Connection Failure", $"{(int)reason}");
            }
        }

        #endregion

        #region Helper Methods

        private void PrepareForSearching()
        {
            _isSearchingMatchMakingSession = true;
            SetConnectionStatus(ConnectionStatus.Connecting);
            ToggleSearchingUIVisibility(true);
        }

        private string GenerateSessionName()
        {
            string newSessionName = "MatchmakingSession_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            return newSessionName;
        }

        private void SetConnectionStatus(ConnectionStatus status)
        {
            Debug.Log($"Setting connection status to {status}");
            ConnectionStatus = status;

            if (!Application.isPlaying)
                return;

            if ((status != ConnectionStatus.Disconnected && status != ConnectionStatus.Failed) ||
                SceneManager.GetActiveScene().buildIndex == LevelManager.MAIN_MENU_SCENE) return;

            SceneManager.LoadScene(LevelManager.MAIN_MENU_SCENE);
            InterfaceManager.Instance.CloseToRoot();
        }


        private void StopRoutine(ref Coroutine routine)
        {
            if (routine != null)
                StopCoroutine(routine);
            routine = null;
        }


        private void ToggleSearchingUIVisibility(bool visible)
        {
            if (_searchingUI == null) return;
            _searchingUI.gameObject.SetActive(visible);
            if (visible) _searchingUI.StartSearching();
            else _searchingUI.StopSearching();
        }

        #endregion

        #region Unused Fusion Methods

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        #endregion
    }
}