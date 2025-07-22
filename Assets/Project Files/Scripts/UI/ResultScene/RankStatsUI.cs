using System.Linq;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.GamePoints;
using Kart.Project_Files.Scripts.Managers.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.ResultScene
{
    public class RankStatsUI : MonoBehaviour
    {
        [SerializeField] private RaceRankStatsCalculation raceRankStatsCalculation;
        [SerializeField] private Image rankImage;
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI scoredPointsText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private PointsSlider pointsSlider;

        public void SetData()
        {
            var playerPoints = GameManager.Instance.PointsTable.GetPoints(RoomPlayer.Local);
            positionText.text = $"{GameManager.Instance.PointsTable.GetPlayerPosition(RoomPlayer.Local)}";
            //scoredPointsText.text = $"{playerPoints/GameManager.Instance.PointsTable.MaxPointsForAllRaces * 100:0.00}%";
            playerNameText.text = $"{RoomPlayer.Local.Username.Value}";
            raceRankStatsCalculation.CalculateRaceRankStats(playerPoints, out RaceRank rank,out var sprite );
            rankImage.sprite = sprite;
        }

        public void SetPoints()
        {
            var playerPoints = GameManager.Instance.PointsTable.GetPoints(RoomPlayer.Local);
            pointsSlider.SetPoints(playerPoints, GameManager.Instance.PointsTable.MaxPointsForAllRaces);
        }
    }
}