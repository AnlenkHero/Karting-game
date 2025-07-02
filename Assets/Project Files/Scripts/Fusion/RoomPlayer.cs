using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.Managers;
using Kart.Project_Files.Scripts.OtherNetworking;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kart.Project_Files.Scripts.Fusion
{
    public class RoomPlayer : NetworkBehaviour
    {
        public enum EGameState
        {
            Lobby,
            GameCutscene,
            GameReady
        }

        public static readonly List<RoomPlayer> Players = new List<RoomPlayer>();

        public static Action<RoomPlayer> PlayerJoined;
        public static Action<RoomPlayer> PlayerLeft;
        public static Action<RoomPlayer> PlayerChanged;

        public static RoomPlayer Local;

        [Networked] public NetworkBool IsReady { get; set; }
        [Networked] public NetworkString<_32> Username { get; set; }
        [Networked] public KartController Kart { get; set; }
        [Networked] public EGameState GameState { get; set; }
        [Networked] public int KartId { get; set; }
        [Networked] public NetworkString<_32> CountryCode { get; set; }
        [Networked] public NetworkBool CountryPrivacy { get; set; }

        public bool IsLeader => Object != null && Object.IsValid && Object.HasStateAuthority;
        public NetworkRunner runner;

        private ChangeDetector _changeDetector;

        public override void Spawned()
        {
            base.Spawned();

            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

            if (Object.HasStateAuthority)
            {
                SetUniqueKartID();
            }

            if (Object.HasInputAuthority)
            {
                Local = this;
                PlayerChanged?.Invoke(this);
                RPC_SetPlayerStats(ClientInfo.Username);
                RPC_SetCountryCode(ClientInfo.CountryCode);
                RPC_SetCountryPrivacy(ClientInfo.CountryPrivacy);
            }

            Players.Add(this);
            PlayerJoined?.Invoke(this);

            DontDestroyOnLoad(gameObject);
        }

        public override void Render()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(IsReady):
                    case nameof(Username):
                        OnStateChanged(this);
                        break;
                }
            }
        }

        private void SetUniqueKartID()
        {
            var usedIds = Players.Select(p => p.KartId).ToList();
            int total = ResourceManager.Instance.kartDefinitions.Length;
            var allIds = Enumerable.Range(0, total);
            var freeIds = allIds.Except(usedIds).ToList();
            
            int chosen = freeIds.Any()
                ? freeIds[Random.Range(0, freeIds.Count)]
                : Random.Range(0, total);

            KartId = chosen;
        }

        [Rpc]
        private void RPC_SetCountryCode(string countryCode)
        {
            if (!string.IsNullOrEmpty(countryCode))
            {
                CountryCode = countryCode;
                Debug.Log($"Country code set for {Username}: {countryCode}");
            }
            else
            {
                Debug.LogError("Failed to load country code.");
            }
        }

        [Rpc]
        private void RPC_SetCountryPrivacy(bool countryPrivacy)
        {
            CountryPrivacy = countryPrivacy;
            Debug.Log($"Country privacy set for {Username}: {countryPrivacy}");
        }

        [Rpc]
        public void RPC_SetPlayerStats(NetworkString<_32> username)
        {
            Username = username;
        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_SetKartId(int id)
        {
            KartId = id;
        }

        [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
        public void RPC_ChangeReadyState(NetworkBool state)
        {
            Debug.Log($"Setting {Object.Name} ready state to {state}");
            IsReady = state;
        }

        private void OnDisable()
        {
            // OnDestroy does not get called for pooled objects
            PlayerLeft?.Invoke(this);
            Players.Remove(this);
        }

        private static void OnStateChanged(RoomPlayer changed) => PlayerChanged?.Invoke(changed);

        public static RoomPlayer GetPlayer(NetworkRunner runner, PlayerRef player)
        {
            var roomPlayer = Players.FirstOrDefault(x => x.Object.InputAuthority == player);
            return roomPlayer != null ? roomPlayer : null;
        }

        public static void RemovePlayer(NetworkRunner runner, PlayerRef p)
        {
            var roomPlayer = Players.FirstOrDefault(x => x.Object.InputAuthority == p);
            if (roomPlayer != null)
            {
                if (roomPlayer.Kart != null)
                    runner.Despawn(roomPlayer.Kart.Object);

                Players.Remove(roomPlayer);
                runner.Despawn(roomPlayer.Object);
            }
        }
    }
}