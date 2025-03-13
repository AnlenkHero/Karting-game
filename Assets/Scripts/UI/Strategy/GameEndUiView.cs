using System.Collections.Generic;
using System.Text;
using Kart.Fusion;
using UnityEngine;
using UnityEngine.UI;
using Kart.ModeStrategy;
using TMPro;

public class GameEndUiView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI standingsText;
    [SerializeField] private GameObject container;
    
    public void ShowEndGameUI(PointsTable standings)
    {
        container.SetActive(true);
        standingsText.text = ComposeStandingsMessage(standings);
    }
    
    private string ComposeStandingsMessage(PointsTable standings)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entry in standings.GetSortedPlayerPointsList())
        {
            sb.AppendLine($"{entry.Key.Id.ToString()}: {entry.Value}");
        }
        return sb.ToString();
    }
    
    public void HideEndGameUI()
    {
        container.SetActive(false);
    }
}