using System.Collections.Generic;
using System.Linq;
using Fusion;
using Kart.ModeStrategy;
using UnityEngine;

namespace Kart.UI
{
    public class LapsUiView : NetworkBehaviour //TODO reorder standings based on their places
    {
        [SerializeField] private LapsStandingView standingViewPrefab;
        [SerializeField] private Transform parent;

        private readonly List<LapsStandingView> _standings = new();
        private List<StandingsEntry> standingsEntry = new ();

        // Called when a standings entry needs to be added or updated.
        public void AddOrUpdateStanding(StandingsEntry[] standing)
        {
            if (standing.Length == 0)
                return;

            // Only the state authority (the host) sends out the update.
          //  if (Object.HasStateAuthority)
            //{
                // For RPCs, you’ll need to send only data types that Fusion supports.
                // Adjust the parameters as needed.
                RpcAddOrUpdateStanding(standing);
            //}
        }

        private void RpcAddOrUpdateStanding(StandingsEntry[] StandingsEntry)
        {
            
            if (Object.HasStateAuthority)
            {
                RpcClear();
                foreach (var standing in StandingsEntry)
                {
                    RpcUpdateStanding(standing.rank, standing.player.ToString(), standing.lastLapTime.ToString(), standing.status.ToString());                    
                }
            }

            if (_standings.Count == standingsEntry.Count)
            {
                for (int i=0; i<standingsEntry.Count; i++)
                {
                    UpdateStandingText(_standings[i], standingsEntry[i].rank, standingsEntry[i].player.ToString(),
                        standingsEntry[i].lastLapTime.ToString(), standingsEntry[i].status.ToString());
                }
                return;   
            }

            int index = 0;
            while (_standings.Count != standingsEntry.Count && _standings.Count < 10)
            {
                var instantiatedObj = Instantiate(standingViewPrefab, parent);

                _standings.Add(instantiatedObj);
                UpdateStandingText(_standings[index], standingsEntry[index].rank, standingsEntry[index].player.ToString(),
                    standingsEntry[index].lastLapTime.ToString(), standingsEntry[index].status.ToString());
                index++;
            }
        }


        private void UpdateStandingText(LapsStandingView view, int rank, string playerName, string lastLapTime,
            string status)
        {
            view.SetText(ComposeStandingMessage(rank, playerName, lastLapTime, status));
        }

        [Rpc]
        private void RpcClear()
        {
            standingsEntry.Clear();
        }
        
        [Rpc]    
        private void RpcUpdateStanding(int rank, string playerName, string lastLapTime, string status)
        {
            var newStanding = new StandingsEntry{rank = rank, player = playerName, lastLapTime = lastLapTime, status = status};
            standingsEntry.Add(newStanding);
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