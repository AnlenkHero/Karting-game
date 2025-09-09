using DG.Tweening;
using Kart.Project_Files.Scripts.AI;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Animations.ResultScene
{
    public class JumpOnPodium : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform body = null!;
        [SerializeField] private Transform landingPoint = null!;

        [Header("Squash & Stretch")]
        [SerializeField] private float squashY = 0.8f;
        [SerializeField] private float stretchY = 1.2f;
        [SerializeField] private float squashDuration = 0.15f;

        [Header("Jump")]
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float jumpDuration = 0.5f;

        [Header("Trigger")]
        [SerializeField] internal CarAIController carAIController;
        public bool Played { get; private set; }

        public void PlayJumpAnimation()
        {
            Played = false;

            var origScale    = body.localScale;
            var squashScale  = new Vector3(origScale.x * (2 - squashY), origScale.y * squashY, origScale.z * (2 - squashY));
            var stretchScale = new Vector3(origScale.x * (2 - stretchY), origScale.y * stretchY, origScale.z * (2 - stretchY));
            
            var landingPos = landingPoint.position;
            var apexPos    = landingPos + Vector3.up * jumpHeight;
            
            var seq = DOTween.Sequence()
                .Append(body.DOScale(squashScale, squashDuration))
                .Append(body.DOScale(stretchScale, squashDuration))
                .Append(carAIController.transform.DOMove(apexPos, jumpDuration * 0.5f).SetEase(Ease.Linear))
                .Join(body.DOScale(origScale, jumpDuration * 0.5f))
                .Append(carAIController.transform.DOMove(landingPos, jumpDuration * 0.5f).SetEase(Ease.Linear))
                .Join(body.DOScale(stretchScale, jumpDuration * 0.5f))
                .Append(body.DOScale(origScale, jumpDuration * 0.5f))
                .OnComplete(() => Played = true);
            
            seq.Play();
        }
    }
}
