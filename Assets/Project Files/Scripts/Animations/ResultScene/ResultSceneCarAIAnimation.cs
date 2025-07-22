using System.Collections;
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
            yield return new WaitUntil(() => thirdPlaceCarAIController.finishedAnimation);
            podiumFall.PlayAnimation();
            yield return new WaitUntil(() => podiumFall.animationFinished);
            
            yield return new WaitUntil(() => firstPlaceCarAIController.finishedAnimation);
            firstPlaceCarAIController.jumpOnPodium.PlayJumpAnimation();
            yield return new WaitUntil(() => firstPlaceCarAIController.jumpOnPodium.Played);
            
            if (secondPlaceCarAIController.gameObject.activeSelf)
            {
                yield return new WaitUntil(() => secondPlaceCarAIController.finishedAnimation);
                secondPlaceCarAIController.jumpOnPodium.PlayJumpAnimation();
                yield return new WaitUntil(() => secondPlaceCarAIController.jumpOnPodium.Played);
            }
            
            if (thirdPlaceCarAIController.gameObject.activeSelf)
            {
                yield return new WaitUntil(() => thirdPlaceCarAIController.finishedAnimation);
                thirdPlaceCarAIController.jumpOnPodium.PlayJumpAnimation();
                yield return new WaitUntil(() => thirdPlaceCarAIController.jumpOnPodium.Played);
            }
            InterfaceManager.Instance.ShowScreen(gameOverUi);
            rankStatsUI.SetData();
            bool rankStatsShown = false;
            imageFaderForRankStats.PlayFadeInQueue(1, 0.5f, false,() => {rankStatsShown = true;});
            yield return new WaitUntil(() => rankStatsShown);
            rankStatsUI.SetPoints();
            yield return new WaitForSeconds(5f);
            imageFaderForRankStats.PlayFadeInQueue(0, 0.5f,true,() => {rankStatsShown = false;});
            yield return new WaitUntil(() => !rankStatsShown);
            imageFaderForPortal.PlayFade(1,1.5f,() =>
            {
                gameEndPortalAnimation.button.interactable = true;
            });
        }
    }
}
