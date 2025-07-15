using System.Collections;
using Kart.Project_Files.Scripts.AI;
using Kart.Project_Files.Scripts.Managers;
using Kart.Project_Files.Scripts.Managers.Game;
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
        }
    }
}
