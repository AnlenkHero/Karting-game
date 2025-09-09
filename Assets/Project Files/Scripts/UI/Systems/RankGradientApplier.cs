using TMPro;
using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Systems
{
    public class RankGradientApplier : MonoBehaviour
    {
        [SerializeField] TMP_ColorGradient goldGradient;
        [SerializeField] TMP_ColorGradient silverGradient;
        [SerializeField] TMP_ColorGradient bronzeGradient;
        
        public void Apply(TextMeshProUGUI text, int rank)
        {
            text.colorGradientPreset = rank switch
            {
                1 => goldGradient,
                2 => silverGradient,
                3 => bronzeGradient,
                _ => null
            };
        }
    }
}