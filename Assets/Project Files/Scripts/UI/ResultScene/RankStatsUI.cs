using System.Linq;
using Kart.Project_Files.Scripts.Animations.ResultScene;
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
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private PointsSlider pointsSlider;
        [SerializeField] private ResultScenePositionUI resultScenePositionUI;

        public void SetData()
        {
            var playerPoints = GameManager.Instance.PointsTable.GetPoints(RoomPlayer.Local);
            playerNameText.text = $"{RoomPlayer.Local.Username.Value}";
            raceRankStatsCalculation.CalculateRaceRankStats(playerPoints, out RaceRank rank,out var sprite );
            resultScenePositionUI.SetData();
            rankImage.sprite = sprite;
        }

        public void SetPoints()
        {
            var playerPoints = GameManager.Instance.PointsTable.GetPoints(RoomPlayer.Local);
            pointsSlider.SetPoints(playerPoints, GameManager.Instance.PointsTable.MaxPointsForAllRaces);
        }
    }
}