using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Controls
{
    public class KartInput : NetworkBehaviour, INetworkRunnerCallbacks
    {
        public struct NetworkInputData : INetworkInput
        {
            public const uint ButtonDrift = 1 << 0;
            public const uint ButtonLookbehind = 1 << 1;
            public const uint UseItem = 1 << 2;

            public uint Buttons;

            public Vector2 Move;

            private bool IsDown(uint button) => (Buttons & button) == button;
            
            public bool IsDriftPressed => IsDown(ButtonDrift);
        }
        
        private PlayerInputActions _inputActions;

        public override void Spawned()
        {
            base.Spawned();

            Runner.AddCallbacks(this);
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);

            DisposeInputs();
            Runner.RemoveCallbacks(this);
        }

        private void OnDestroy()
        {
            DisposeInputs();
        }

        private void DisposeInputs()
        {
            _inputActions.Dispose();
            _inputActions = null;
        }
        

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var userInput = new NetworkInputData();
            userInput.Move = _inputActions.Player.Move.ReadValue<Vector2>();
            if (_inputActions.Player.HandBrake.ReadValue<float>() != 0.0f)
            {
                userInput.Buttons |= NetworkInputData.ButtonDrift;
            }
            
            input.Set(userInput);
        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
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

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
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
    }
}