using System.Collections.Generic;
using System.Linq;
using Fusion;
using Kart.ModeStrategy;
using UnityEngine;

namespace Kart.UI
{
    public class LapsUiView : NetworkBehaviour
    {
        [SerializeField] private LapsStandingView standingViewPrefab;
        [SerializeField] private Transform parent;

        private readonly List<LapsStandingView> standings = new();
        private readonly List<StandingsEntry> standingsEntry = new();

        public void AddOrUpdateStanding(List<StandingsEntry> standing)
        {
            if (standing.Count == 0)
                return;

            RpcAddOrUpdateStanding(standing);
        }

        private void RpcAddOrUpdateStanding(List<StandingsEntry> standingsEntries)
        {
            if (Object.HasStateAuthority)
            {
                RpcClear();
                foreach (var standing in standingsEntries)
                {
                    RpcUpdateStanding(standing.rank, standing.player, standing.lastLapTime,
                        standing.status);
                }
            }

            if (standings.Count == standingsEntry.Count)
            {
                for (int i = 0; i < standingsEntry.Count; i++)
                {
                    UpdateStandingText(standings[i], standingsEntry[i].rank, standingsEntry[i].player,
                        standingsEntry[i].lastLapTime, standingsEntry[i].status);
                }

                return;
            }

            int index = 0;
            while (standings.Count != standingsEntry.Count && standings.Count < 10)
            {
                var instantiatedObj = Instantiate(standingViewPrefab, parent);

                standings.Add(instantiatedObj);
                UpdateStandingText(standings[index], standingsEntry[index].rank,
                    standingsEntry[index].player,
                    standingsEntry[index].lastLapTime, standingsEntry[index].status);
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
            var newStanding = new StandingsEntry
                { rank = rank, player = playerName, lastLapTime = lastLapTime, status = status };
            standingsEntry.Add(newStanding);
        }

        private string ComposeStandingMessage(int rank, string playerName, string lastLapTime, string status)
        {
            string message = $"{rank}. {playerName} - {lastLapTime}";
            if (!string.IsNullOrEmpty(status) && status == "Finished")
                message += $" - {status}";
            return message;
        }
    }
}