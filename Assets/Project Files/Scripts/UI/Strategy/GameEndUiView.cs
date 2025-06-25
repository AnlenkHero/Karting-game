using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Helpers;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.UI.Effects;
using Kart.Project_Files.Scripts.UI.Systems;
using TMPro;
using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Strategy
{
    public class GameEndUiView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI header;
        [SerializeField] private GameObject container;
        [SerializeField] private GameEndUiStanding standingPrefab;
        [SerializeField] private Transform standingsParent;
        [SerializeField] private EndRaceVignette endRaceVignette;
        [SerializeField] private RankGradientApplier rankGradientApplier;
        [SerializeField] private float animationDuration = 0.5f;
        private List<GameEndUiStanding> _standings = new();

        public void ShowEndGameUI(PointsTable standings, string headerText = null)
        {
            endRaceVignette.PlayVignetteFadeIn(() => ShowEndGameUIAnimation(standings, headerText));
        }

        private void ShowEndGameUIAnimation(PointsTable standings, string headerText = null)
        {
            container.SetActive(true);
            header.text = $"{GameManager.CurrentTrack.trackDefinition.trackName} RACE RESULTS";
            if (headerText != null)
            {
                header.text = headerText;
            }
            
            header.DOFade(1f, animationDuration).SetEase(Ease.InCubic).OnComplete(() =>
            {
                StartCoroutine(ComposeStandingsMessage(standings));
            });
        }

        private IEnumerator ComposeStandingsMessage(PointsTable standings)
        {
            standingsParent.ClearExistingElementsInParent();
            var sortedList = standings.GetSortedPlayerPointsList();
            for (int playerPoints = 0; playerPoints < sortedList.Count; playerPoints++)
            {
                int position = playerPoints + 1;
                yield return new WaitForSeconds(0.2f);
                var standing = Instantiate(standingPrefab, standingsParent);
                standing.SetData(rankGradientApplier,sortedList[playerPoints].Key.Username.ToString(), position,
                    sortedList[playerPoints].Value, null);
                standing.PlayFadeInAnimation();
                _standings.Add(standing);
            }
        }

        public void HideStandingsAnimation(Action onStandingsHidden = null)
        {
            StartCoroutine(HideStandingsAnimationCoroutine(onStandingsHidden));
        }

        private IEnumerator HideStandingsAnimationCoroutine(Action onStandingsHidden = null)
        {
            foreach (var standing in _standings)
            {
                standing.PlayFadeOutAnimation();
                yield return new WaitForSeconds(0.2f);
            }

            header.DOFade(0f, animationDuration).SetEase(Ease.InCubic).OnComplete(() =>
            {
                container.SetActive(false);
                onStandingsHidden?.Invoke();
            });
        }

        public void HideEndGameUI()
        {
            container.SetActive(false);
        }
    }
}