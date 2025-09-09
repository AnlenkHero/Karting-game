using DG.Tweening;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Animations.ResultScene
{
    public class PodiumFall : MonoBehaviour
    {
        [SerializeField] private Transform podium;
        [SerializeField] private Transform desiredFallPosition;
        public bool animationFinished;

        public void PlayAnimation()
        {
            animationFinished = false;
            podium.gameObject.SetActive(true);
            Vector3 targetLocalPos = podium.parent != null
                ? podium.parent.InverseTransformPoint(desiredFallPosition.position)
                : desiredFallPosition.position;

            var seq = DOTween.Sequence()
                .Append(podium.DOMove(targetLocalPos, 3f).SetEase(Ease.OutBounce))
                .OnComplete(() => animationFinished = true);

            seq.Play();
        }
    }
}