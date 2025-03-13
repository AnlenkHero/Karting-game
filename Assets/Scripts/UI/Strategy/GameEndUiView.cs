using System.Text;
using Kart.Fusion;
using UnityEngine;
using Kart.UI.Strategy;
using TMPro;

public class GameEndUiView : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private TextMeshProUGUI standingsText;
    [SerializeField] private GameEndUiStanding standingPrefab;
    [SerializeField] private Transform standingsParent;

    public void ShowEndGameUI(PointsTable standings)
    {
        container.SetActive(true);
        //standingsText.text = ComposeStandingsMessage(standings);
        ComposeStandingsMessage(standings);
    }

    private void ComposeStandingsMessage(PointsTable standings)
    {
        StringBuilder sb = new StringBuilder();
        var sortedList = standings.GetSortedPlayerPointsList();
        for (int playerPoints = 0; playerPoints < sortedList.Count; playerPoints++)
        {
            int position = playerPoints + 1;
            var nigga = Instantiate(standingPrefab, standingsParent);
            nigga.SetData(sortedList[playerPoints].Key.Id.ToString(), position, sortedList[playerPoints].Value, null);
        }

        //return sb.ToString();
    }

    public void HideEndGameUI()
    {
        container.SetActive(false);
    }
}