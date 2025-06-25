using System.Globalization;
using DG.Tweening;
using Kart.Project_Files.Scripts.UI.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Strategy
{
    public class GameEndUiStanding : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private TextMeshProUGUI position;
        [SerializeField] private TextMeshProUGUI points;
        [SerializeField] private Image icon;
        [SerializeField] private float fadeDuration = 0.5f;
        public void SetData(RankGradientApplier rankGradientApplier, string playerNameText, int positionValue, float pointsValue, Sprite iconTexture)
        {
            playerName.text = playerNameText;
            position.text = positionValue.ToString();
            points.text = pointsValue.ToString(CultureInfo.InvariantCulture);
            icon.sprite = iconTexture;
            rankGradientApplier.Apply(playerName, positionValue);
            rankGradientApplier.Apply(position, positionValue);
            rankGradientApplier.Apply(points, positionValue);
        }

        public void PlayFadeInAnimation()
        {
            var c = playerName.color;
            c.a = 0f;
            playerName.color = c;
            position.color = c;
            points.color = c;
            icon.color = c;

            playerName.DOFade(1f, 0.5f).SetEase(Ease.InCubic);
            position.DOFade(1f, 0.5f).SetEase(Ease.InCubic);
            points.DOFade(1f, 0.5f).SetEase(Ease.InCubic);
            icon.DOFade(1f, 0.5f).SetEase(Ease.InCubic);
        }

        public void PlayFadeOutAnimation()
        {
            playerName.DOFade(0f, 0.5f).SetEase(Ease.InCubic);
            position.DOFade(0f, 0.5f).SetEase(Ease.InCubic);
            points.DOFade(0f, 0.5f).SetEase(Ease.InCubic);
            icon.DOFade(0f, 0.5f).SetEase(Ease.InCubic);
        }
    }
}