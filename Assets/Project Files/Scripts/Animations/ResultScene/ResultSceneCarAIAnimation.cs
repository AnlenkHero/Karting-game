using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kart.Project_Files.Scripts.Managers;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.Managers.Interface;
using Kart.Project_Files.Scripts.UI.Effects;
using Kart.Project_Files.Scripts.UI.ResultScene;
using Kart.Project_Files.Scripts.UI.Screens;
using TMPro;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Animations.ResultScene
{
    public class ResultSceneCarAIAnimation : MonoBehaviour
    {
        [SerializeField] private PodiumFall podiumFall;
        [SerializeField] private CarAIController firstPlaceCarAIController;
        [SerializeField] private CarAIController secondPlaceCarAIController;
        [SerializeField] private CarAIController thirdPlaceCarAIController;
        [SerializeField] private TextMeshPro firstPlaceText;
        [SerializeField] private TextMeshPro secondPlaceText;
        [SerializeField] private TextMeshPro thirdPlaceText;
        [SerializeField] private UIScreen gameOverUi;
        [SerializeField] private GameEndPortalAnimation gameEndPortalAnimation;
        [SerializeField] private RankStatsUI rankStatsUI;
        [SerializeField] private ImageFader imageFaderForRankStats;
        [SerializeField] private ImageFader imageFaderForPortal;
        private bool _animationPlayer;
        private void Awake()
        {
            var players = GameManager.Instance.PointsTable.GetSortedPlayerPointsList();
            
            firstPlaceText?.SetText(players[0].Key.Username.Value);
            if (players.Count > 1)  secondPlaceText?.SetText(players[1].Key.Username.Value);
            if (players.Count > 2)   thirdPlaceText?.SetText(players[2].Key.Username.Value);
            
            Instantiate(ResourceManager.Instance
                    .kartDefinitions[players[0].Key.KartId].kartModel,
                firstPlaceCarAIController.kartParent);
            firstPlaceCarAIController.gameObject.SetActive(true);

            if (players.Count > 1)
            {
                Instantiate(ResourceManager.Instance
                        .kartDefinitions[players[1].Key.KartId].kartModel,
                    secondPlaceCarAIController.kartParent);
                secondPlaceCarAIController.gameObject.SetActive(true);
            }

            if (players.Count > 2)
            {
                Instantiate(ResourceManager.Instance
                        .kartDefinitions[players[2].Key.KartId].kartModel,
                    thirdPlaceCarAIController.kartParent);
                thirdPlaceCarAIController.gameObject.SetActive(true);
            }
        }
        private void Start()
        {
            StartCoroutine(PlayPodiumSequence());
        }

        private IEnumerator PlayPodiumSequence()
        {
            var cars = new List<(CarAIController controller, JumpOnPodium jump)>()
                {
                    (firstPlaceCarAIController, firstPlaceCarAIController.jumpOnPodium),
                    (secondPlaceCarAIController, secondPlaceCarAIController.jumpOnPodium),
                    (thirdPlaceCarAIController, thirdPlaceCarAIController.jumpOnPodium)
                }
                .Where(x => x.controller.gameObject.activeSelf)
                .ToList();
            
            yield return new WaitUntil(() => cars.Last().controller.finishedAnimation);
            
            podiumFall.PlayAnimation();
            yield return new WaitUntil(() => podiumFall.animationFinished);
            
            foreach (var (ctrl, jump) in cars)
            {
                yield return new WaitUntil(() => ctrl.finishedAnimation);
                jump.PlayJumpAnimation();
                yield return new WaitUntil(() => jump.Played);
            }
            yield return new WaitForSeconds(1f);
            
            InterfaceManager.Instance.ShowScreen(gameOverUi);
            
            rankStatsUI.SetData();
            bool statsIn = false;
            imageFaderForRankStats.PlayFadeInQueue(1, 0.5f, false, () => statsIn = true);
            yield return new WaitUntil(() => statsIn);

            rankStatsUI.SetPoints();
            yield return new WaitForSeconds(5f);
            
            imageFaderForRankStats.PlayFadeInQueue(0, 0.5f, true, () => statsIn = false);
            yield return new WaitUntil(() => !statsIn);
            
            imageFaderForPortal.PlayFade(1, 1.5f, () => gameEndPortalAnimation.button.interactable = true);
        }
    }
}
