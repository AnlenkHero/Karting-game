using Kart.Fusion;
using Kart.Helpers;
using UnityEngine;
using Kart.UI.Strategy;

public class GameEndUiView : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private GameEndUiStanding standingPrefab;
    [SerializeField] private Transform standingsParent;

    public void ShowEndGameUI(PointsTable standings)
    {
        container.SetActive(true);
        ComposeStandingsMessage(standings);
    }

    private void ComposeStandingsMessage(PointsTable standings)
    {
        standingsParent.ClearExistingElementsInParent();
        var sortedList = standings.GetSortedPlayerPointsList();
        
        for (int playerPoints = 0; playerPoints < sortedList.Count; playerPoints++)
        {
            int position = playerPoints + 1;
            var standing = Instantiate(standingPrefab, standingsParent);
            standing.SetData(sortedList[playerPoints].Key.Username.ToString(), position, sortedList[playerPoints].Value, null);
        }
    }

    public void HideEndGameUI()
    {
        container.SetActive(false);
    }
}