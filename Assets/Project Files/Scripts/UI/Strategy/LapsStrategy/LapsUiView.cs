using System.Collections;
using System.Collections.Generic;
using Fusion;
using Kart.Project_Files.Scripts.ModeStrategy.LapStrategy;
using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Strategy.LapsStrategy
{
    public class LapsUiView : NetworkBehaviour
    {
        [SerializeField] private LapsStandingView standingViewPrefab;
        [SerializeField] private Transform parent;
        
        private List<LapsStandingView> standings = new ();
        private readonly List<LapStandings> standingsEntry = new ();

        private int expectedStandingsCount;
        private int updatesReceived;

        private bool isDelay;
        private Coroutine delayCoroutine;
        private Coroutine uiUpdateCoroutine;
        
        private const int MaxStandings = 10;

        private void Start()
        {
            for (int i = 0; i < MaxStandings; i++)
            {
                var view = Instantiate(standingViewPrefab, parent);
                view.gameObject.SetActive(false);
                standings.Add(view);
            }
        }

        public void AddOrUpdateStanding(List<LapStandings> standing)
        {
            if (standing == null || standing.Count == 0 || !HasStateAuthority || isDelay)
                return;

            StartCoroutine(DelayedUpdateUI());
            RpcClear();

            RpcSetExpectedStandingsCount(standing.Count);

            for (int i = 0; i < standing.Count; i++)
            {
                var entry = standing[i];
                RpcUpdateStanding(i, entry.rank, entry.playerName, entry.lastLapTime, entry.status);
            }
        }

        #region RPC Methods

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcClear()
        {
            standingsEntry.Clear();
            updatesReceived = 0;
            expectedStandingsCount = 0;

            UpdateStandingsUI();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcSetExpectedStandingsCount(int count)
        {
            expectedStandingsCount = count;
            standingsEntry.Clear();
            
            for (int i = 0; i < count; i++)
            {
                standingsEntry.Add(new LapStandings());
            }

            updatesReceived = 0;

            StartUIDebounce();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RpcUpdateStanding(int index, int rank, string playerName, string lastLapTime, string status)
        {
            if (index >= 0 && index < standingsEntry.Count)
            {
                standingsEntry[index] = new LapStandings
                {
                    rank = rank,
                    playerName = playerName,
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

        private IEnumerator DelayedUpdateUI()
        {
            isDelay = true;
            yield return new WaitForSeconds(0.2f);
            isDelay = false;
        }

        #endregion

        private void UpdateStandingsUI()
        {
            for (int i = 0; i < standings.Count; i++)
            {
                if (i < standingsEntry.Count)
                {
                    standings[i].gameObject.SetActive(true);
                    UpdateStandingText(standings[i],
                        standingsEntry[i].rank,
                        standingsEntry[i].playerName,
                        standingsEntry[i].lastLapTime,
                        standingsEntry[i].status);
                }
                else
                {
                    standings[i].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateStandingText(LapsStandingView view, int rank, string playerName, string lastLapTime, string status)
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

        public void DisableUI()
        {
            parent.gameObject.SetActive(false);
        }
    }
}
