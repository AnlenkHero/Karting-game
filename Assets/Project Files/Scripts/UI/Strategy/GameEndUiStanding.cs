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

        public void SetData(string playerName, int position, float points, Sprite icon )
        {
            this.playerName.text = playerName;
            this.position.text = position.ToString();
            this.points.text = points.ToString();
            this.icon.sprite = icon;
        }
    }
}