using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using Kart;
using Kart.Fusion;
using Kart.Helpers;
using Kart.Managers;
using Kart.UI;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [SerializeField] private LevelManager _levelManager;
    
    [SerializeField] private DummySearchingUI _searchingUI;
    
    public static ConnectionStatus ConnectionStatus = ConnectionStatus.Disconnected;

    private GameMode _gameMode;
    private NetworkRunner _runner;
    private FusionObjectPoolRoot _pool;
    
    public static GameLauncher Instance => Singleton<GameLauncher>.Instance;

    private void Start()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        QualitySettings.vSyncCount = 1;

        DontDestroyOnLoad(gameObject);

        // Load your main menu scene at startup
        SceneManager.LoadScene(LevelManager.MAIN_MENU_SCENE);
    }

    public void SetCreateLobby() => _gameMode = GameMode.Host;
    public void SetJoinLobby() => _gameMode = GameMode.Client;

    /// <summary>
    /// Called by a button or similar. Joins (or creates) a matchmaking lobby.
    /// </summary>
    public async void JoinOrCreateLobby()
    {
        SetConnectionStatus(ConnectionStatus.Connecting);

        // 2) Show the searching UI while matchmaking
        if (_searchingUI)
        {
            _searchingUI.gameObject.SetActive(true);
            _searchingUI.StartSearching();
        }

        if (_runner != null)
            LeaveSession();

        GameObject go = new GameObject("Session");
        DontDestroyOnLoad(go);

        _runner = go.AddComponent<NetworkRunner>();
        var sim3D = go.AddComponent<RunnerSimulatePhysics3D>();
        sim3D.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;
        
        _gameMode = GameMode.AutoHostOrClient;
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        _pool = go.AddComponent<FusionObjectPoolRoot>();

        Debug.Log($"Created gameobject {go.name} - joining matchmaking lobby");
        
        var joinLobbyResult = await _runner.JoinSessionLobby(SessionLobby.Custom, "MyMatchmakingLobby");
        if (joinLobbyResult.Ok)
        {
            Debug.Log("Successfully joined lobby: MyMatchmakingLobby. Awaiting session list update...");
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

        // Filter sessions: open, not full, name starts with "MyMatchmakingSession"
        SessionInfo targetSession = sessionList
            .Where(session => session.IsOpen)
            .Where(session => session.PlayerCount < session.MaxPlayers)
            .FirstOrDefault(session => session.Name.StartsWith("MyMatchmakingSession"));

        if (targetSession != null)
        {
            Debug.Log("Found joinable session: " + targetSession.Name);
            StartGameSession(targetSession.Name, enableCreation: false);
        }
        else
        {
            Debug.Log("No joinable session found. Creating a new session.");
            string newSessionName = "MyMatchmakingSession_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            StartGameSession(newSessionName, enableCreation: true);
        }
    }
    
    private async void StartGameSession(string sessionName, bool enableCreation)
    {
        Debug.Log($"Starting game session: {sessionName} (Enable Creation: {enableCreation})");

        var startArgs = new StartGameArgs
        {
            GameMode = _gameMode,
            SessionName = sessionName,
            ObjectProvider = _pool,
            SceneManager = _levelManager,
            PlayerCount = 2,
            EnableClientSessionCreation = enableCreation,
            MatchmakingMode = MatchmakingMode.FillRoom
        };

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

    private void SetConnectionStatus(ConnectionStatus status)
    {
        Debug.Log($"Setting connection status to {status}");
        ConnectionStatus = status;

        if (!Application.isPlaying)
            return;

        if (status == ConnectionStatus.Disconnected || status == ConnectionStatus.Failed)
        {
            SceneManager.LoadScene(LevelManager.MAIN_MENU_SCENE);
            UIScreen.BackToInitial();
        }
    }

    public void LeaveSession()
    {
        if (_runner != null)
            _runner.Shutdown();
        else
            SetConnectionStatus(ConnectionStatus.Disconnected);
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server");
        SetConnectionStatus(ConnectionStatus.Connected);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("Disconnected from server");
        LeaveSession();
        SetConnectionStatus(ConnectionStatus.Disconnected);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        if (runner.TryGetSceneInfo(out var scene) && scene.SceneCount > 0)
        {
            Debug.LogWarning($"Refused connection requested by {request.RemoteAddress}");
            request.Refuse();
        }
        else
            request.Accept();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log($"Connect failed {reason}");
        LeaveSession();
        SetConnectionStatus(ConnectionStatus.Failed);
        (string status, string message) = ConnectFailedReasonToHuman(reason);
        _disconnectUI.ShowMessage(status, message);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} Joined!");
        if (runner.IsServer)
        {
            if (_gameMode == GameMode.Host)
                runner.Spawn(_gameManagerPrefab, Vector3.zero, Quaternion.identity);

            var roomPlayer = runner.Spawn(_roomPlayerPrefab, Vector3.zero, Quaternion.identity, player);
            roomPlayer.GameState = RoomPlayer.EGameState.Lobby;

            // 3) Check if we reached MaxPlayers -> automatically start the game
            if (runner.SessionInfo.PlayerCount == runner.SessionInfo.MaxPlayers)
            {
                GameStarted();
            }
        }

        SetConnectionStatus(ConnectionStatus.Connected);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"{player.PlayerId} disconnected.");
        RoomPlayer.RemovePlayer(runner, player);
        SetConnectionStatus(ConnectionStatus);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"OnShutdown {shutdownReason}");
        SetConnectionStatus(ConnectionStatus.Disconnected);

        (string status, string message) = ShutdownReasonToHuman(shutdownReason);
        _disconnectUI.ShowMessage(status, message);

        RoomPlayer.Players.Clear();

        if (_runner)
            Destroy(_runner.gameObject);

        _pool?.ClearPools();
        _pool = null;
        _runner = null;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    /// <summary>
    /// Called by the server once all slots are filled. We close the session and hide the searching UI.
    /// You can also load a game scene here if desired.
    /// </summary>
    private void GameStarted()
    {
        // 4) Mark the session as closed
        _runner.SessionInfo.IsOpen = false;

        // 5) Hide the searching UI
        if (_searchingUI)
        {
            _searchingUI.gameObject.SetActive(false);
            _searchingUI.StopSearching();
        }

        // 6) Optionally load your game scene or do any other "start" logic
        Debug.Log("All players joined. Starting the game now...");
        LevelManager.LoadTrack(ResourceManager.Instance.tracks[0].buildIndex);
    }

    private static (string, string) ShutdownReasonToHuman(ShutdownReason reason)
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

    private static (string, string) ConnectFailedReasonToHuman(NetConnectFailedReason reason)
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
}
