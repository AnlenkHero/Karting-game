using System.Collections;
using System.Collections.Generic;
using Fusion;
using Kart.ModeStrategy;
using UnityEngine;

namespace Kart.UI.Strategy
{
    public class LapsUiView : NetworkBehaviour
    {
        [SerializeField] private LapsStandingView standingViewPrefab;
        [SerializeField] private Transform parent;

        private readonly List<LapsStandingView> standings = new ();
        private readonly List<StandingsEntry> standingsEntry = new ();

        private int expectedStandingsCount;
        private int updatesReceived;

        private Coroutine uiUpdateCoroutine;

        public void AddOrUpdateStanding(List<StandingsEntry> standing)
        {
            if (standing == null || standing.Count == 0 || !HasStateAuthority)
                return;

            RpcClear();

            RpcSetExpectedStandingsCount(standing.Count);

            for (int i = 0; i < standing.Count; i++)
            {
                var entry = standing[i];
                RpcUpdateStanding(i, entry.rank, entry.player, entry.lastLapTime, entry.status);
            }
        }

        #region RPC Methods

        [Rpc]
        private void RpcClear()
        {
            standingsEntry.Clear();
            updatesReceived = 0;
            expectedStandingsCount = 0;

            UpdateStandingsUI();
        }

        [Rpc]
        private void RpcSetExpectedStandingsCount(int count)
        {
            expectedStandingsCount = count;
            standingsEntry.Clear();
            
            for (int i = 0; i < count; i++)
            {
                standingsEntry.Add(new StandingsEntry());
            }

            updatesReceived = 0;

            StartUIDebounce();
        }

        [Rpc]
        private void RpcUpdateStanding(int index, int rank, string playerName, string lastLapTime, string status)
        {
            if (index >= 0 && index < standingsEntry.Count)
            {
                standingsEntry[index] = new StandingsEntry
                {
                    rank = rank,
                    player = playerName,
                    lastLapTime = lastLapTime,
                    status = status
                };
                updatesReceived++;
            }

            if (updatesReceived >= expectedStandingsCount)
            {
                StopUIDebounce();
                UpdateStandingsUI();
            }
            else
            {
                StartUIDebounce();
            }
        }

        #endregion

        #region Debounce Helpers

        private void StartUIDebounce()
        {
            if (uiUpdateCoroutine != null)
            {
                StopCoroutine(uiUpdateCoroutine);
                uiUpdateCoroutine = null;
            }

            uiUpdateCoroutine = StartCoroutine(DebouncedUpdateUI());
        }

        private void StopUIDebounce()
        {
            if (uiUpdateCoroutine == null) return;
            
            StopCoroutine(uiUpdateCoroutine);
            uiUpdateCoroutine = null;
        }

        private IEnumerator DebouncedUpdateUI()
        {
            yield return new WaitForSeconds(0.2f);
            UpdateStandingsUI();
        }

        #endregion
        
        private void UpdateStandingsUI()
        {
            while (standings.Count < standingsEntry.Count && standings.Count < 10)
            {
                var view = Instantiate(standingViewPrefab, parent);
                standings.Add(view);
            }
            
            for (int i = 0; i < standingsEntry.Count; i++)
            {
                if (i >= standings.Count) continue;
                
                standings[i].gameObject.SetActive(true);
                UpdateStandingText(standings[i],
                    standingsEntry[i].rank,
                    standingsEntry[i].player,
                    standingsEntry[i].lastLapTime,
                    standingsEntry[i].status);
            }
            
            for (int i = standingsEntry.Count; i < standings.Count; i++)
            {
                standings[i].gameObject.SetActive(false);
            }
        }


        private void UpdateStandingText(LapsStandingView view, int rank, string playerName, string lastLapTime,
            string status)
        {
            view.SetText(ComposeStandingMessage(rank, playerName, lastLapTime, status));
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