using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Fusion
{
    public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        public NetworkPrefabRef PlayerPrefab;
        public NetworkPrefabRef kartPrefab;
        public Button niggaButton;
        public Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new();
        [SerializeField] private NetworkRunner _runner;

        private void Awake()
        {
            niggaButton.onClick.AddListener(nigga);
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($"Player {player} Joined!");
            if (runner.IsServer)
            { 
                runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity, player);
            }
        }

        public void nigga()
        {
            foreach (var player in RoomPlayer.Players)
            {
                player.Spawn(_runner, player, kartPrefab);
            }
        }


        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!_spawnedPlayers.TryGetValue(player, out var networkObject)) return;

            runner.Despawn(networkObject);
            _spawnedPlayers.Remove(player);
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
            ArraySegment<byte> data)
        {
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }


        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        async void StartGame(GameMode gameMode)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = gameMode,
                SessionName = "TestRoom",
                Scene = null, // Pass the build index directly
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            });
        }
    }
}