using Fusion;
using Kart.Project_Files.Scripts.UI.Systems;
using TMPro;
using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Strategy.LapsStrategy
{
    public class LapsStandingView : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI standingText;
        [SerializeField] private RankGradientApplier gradientApplier;
        
        public void SetText(string text, int rank)
        {
            standingText.text = text;
            gradientApplier.Apply(standingText, rank);
        }
    }
}