using System.Collections.Generic;
using Fusion;
using Kart.ModeStrategy;
using UnityEngine;

namespace Kart.UI
{
    public class LapsUiView : NetworkBehaviour //TODO reorder standings based on their places
    {
        [SerializeField] private LapsStandingView standingViewPrefab;
        [SerializeField] private Transform parent;

        private readonly Dictionary<NetworkBehaviourId, LapsStandingView> _standings = new();

        // Called when a standings entry needs to be added or updated.
        public void AddOrUpdateStanding(StandingsEntry standing)
        {
            if (standing.player == null)
                return;

            // Only the state authority (the host) sends out the update.
            if (Object.HasStateAuthority)
            {
                // For RPCs, you’ll need to send only data types that Fusion supports.
                // Adjust the parameters as needed.
                RpcAddOrUpdateStanding(standing.player.Id, standing);
            }
        }

        private void RpcAddOrUpdateStanding(NetworkBehaviourId playerId, StandingsEntry standingsEntry)
        {
            if (_standings.TryGetValue(playerId, out var standingView))
            {
                RpcUpdateStanding(standingView, standingsEntry.rank, standingsEntry.player.name,
                standingsEntry.additionalInfo["LastLapTime"], standingsEntry.additionalInfo["Status"]);
                return;
            }
            
            var instantiatedObj = Runner.Spawn(standingViewPrefab);
            
            RpcSetParent(instantiatedObj);
            
            RpcUpdateStanding(instantiatedObj, standingsEntry.rank, standingsEntry.player.name,
                standingsEntry.additionalInfo["LastLapTime"], standingsEntry.additionalInfo["Status"]);
            _standings.Add(playerId, instantiatedObj);
        }

        [Rpc]
        private void RpcUpdateStanding(LapsStandingView view, int rank, string playerName, string lastLapTime,
            string status)
        {
            view.SetText(ComposeStandingMessage(rank, playerName, lastLapTime, status));
        }

        [Rpc]
        private void RpcSetParent(LapsStandingView view)
        {
            view.transform.SetParent(parent, false);
        }

        private string ComposeStandingMessage(int rank, string playerName, string lastLapTime, string status)
        {
            string message = $"{rank}. {playerName} - {lastLapTime}";
            if (!string.IsNullOrEmpty(status) && status == "Finished")
                message += $" - {status}";
            return message;
        }    }
}