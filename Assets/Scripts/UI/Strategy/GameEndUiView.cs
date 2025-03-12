using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Kart.ModeStrategy;

public class GameEndUiView : MonoBehaviour
{
    [SerializeField] private Text standingsText;
    [SerializeField] private GameObject container;
    
    public void ShowEndGameUI(List<StandingsEntry> standings)
    {
        container.SetActive(true);
        standingsText.text = ComposeStandingsMessage(standings);
    }
    
    private string ComposeStandingsMessage(List<StandingsEntry> standings)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entry in standings)
        {
            string statusText = (!string.IsNullOrEmpty(entry.status) && entry.status == "Finished") ? $" - {entry.status}" : "";
            sb.AppendLine($"{entry.rank}. {entry.player} - {entry.lastLapTime}{statusText}");
        }
        return sb.ToString();
    }
    
    public void HideEndGameUI()
    {
        container.SetActive(false);
    }
}