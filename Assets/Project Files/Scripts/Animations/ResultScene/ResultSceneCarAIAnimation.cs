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
