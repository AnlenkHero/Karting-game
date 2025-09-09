using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Fusion;
using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.Extensions;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.ModeStrategy.LapStrategy;
using Kart.Project_Files.Scripts.UI.Systems;
using TMPro;
using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Strategy.LapsStrategy
{
    public class LapsUiView : NetworkBehaviour
    {
        [SerializeField] private LapsStandingView standingViewPrefab;
        [SerializeField] private Transform parent;
        [SerializeField] private TextMeshProUGUI currentLapText;
        [SerializeField] private TextMeshProUGUI currentRankText;
        [SerializeField] private TextMeshProUGUI currentLastLapTimeText;
        [SerializeField] private TextMeshProUGUI bestLapTimeText;
        [SerializeField] private GameObject container;
        [SerializeField] private RankGradientApplier gradientApplier;
        [SerializeField] private SpectatorController spectatorController;
        private List<LapsStandingView> standings = new();
        private readonly List<LapStandings> standingsEntry = new();

        private int expectedStandingsCount;
        private int updatesReceived;

        private bool isDelay;
        private Coroutine delayCoroutine;
        private Coroutine uiUpdateCoroutine;

        private const int MaxStandings = 10;

        public override void Spawned()
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
                RpcUpdateStanding(i, entry.playerId, entry.lapsCompleted, entry.rank, entry.playerName,
                    entry.lastLapTime, entry.status);
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
        private void RpcUpdateStanding(int index, string playerId, string lapsCompleted, int rank, string playerName,
            string lastLapTime, string status)
        {
            if (index >= 0 && index < standingsEntry.Count)
            {
                standingsEntry[index] = new LapStandings
                {
                    rank = rank,
                    playerId = playerId,
                    lapsCompleted = lapsCompleted,
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
            ShowBestLapTime();
            for (int i = 0; i < standings.Count; i++)
            {
                if (i < standingsEntry.Count)
                {
                    standings[i].gameObject.SetActive(true);
                    UpdateStandingText(standings[i],
                        standingsEntry[i].playerId,
                        standingsEntry[i].lapsCompleted,
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

        private void ShowBestLapTime()
        {
            float best = float.MaxValue;
            string playerName = "";

            foreach (var s in standingsEntry)
            {
                var tRaw = s.lastLapTime;
                if (!float.TryParse(tRaw.TrimEnd('s').Replace(',', '.'), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out var t)
                    || t <= 0 || t >= best)
                    continue;

                best = t;
                playerName = s.playerName;
            }

            if (best < float.MaxValue)
            {
                bestLapTimeText.text =
                    $"BEST LAP: \n{playerName} {best.ToString(CultureInfo.InvariantCulture).ToRaceFormat()}";
            }
            else if(Mathf.Approximately(best, float.MaxValue))
            {
                bestLapTimeText.text = "BEST LAP:\nN/A";
            }
        }


        private void UpdateStandingText(
            LapsStandingView view,
            string playerId,
            string lapsCompleted,
            int rank,
            string playerName,
            string lastLapTime,
            string status)
        {
            var formatted = lastLapTime.ToRaceFormat();
            var targetId = spectatorController.canSpectate
                ? spectatorController.CurrentSpectatedPlayerId
                : RoomPlayer.Local.Id.ToString();

            if (playerId == targetId)
            {
                currentLapText.text        = $"LAP {lapsCompleted}";
                gradientApplier.Apply(currentRankText, rank);
                currentRankText.text       = rank.ToOrdinal();
                currentLastLapTimeText.text = lastLapTime.ToRaceFormat();
            }

            view.SetText(
                ComposeStandingMessage(rank, playerName, formatted, status),
                rank
            );
        }

        private string ComposeStandingMessage(int rank, string playerName, string lastLapTime, string status)
        {
            string message = $"{rank}. {playerName}";
            if (!string.IsNullOrEmpty(status) && status == "FINISHED")
                message += $" - {status}";
            return message;
        }

        public void DisableUI()
        {
            container.SetActive(false);
        }
    }
}