using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Managers.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.Animations.ResultScene
{
    public class ResultScenePositionUI : MonoBehaviour
    {
        [SerializeField] private Image positionImage;
        [SerializeField] private Sprite firstPlaceSprite;
        [SerializeField] private Sprite secondPlaceSprite;
        [SerializeField] private Sprite thirdPlaceSprite;

        public void SetData()
        {
            int localPlayerPosition = GameManager.Instance.PointsTable.GetPlayerPosition(RoomPlayer.Local);
            if(localPlayerPosition > 3)
            {
                positionImage.gameObject.SetActive(false);
                return;
            }

            positionImage.sprite = localPlayerPosition switch
            {
                1 => firstPlaceSprite,
                2 => secondPlaceSprite,
                3 => thirdPlaceSprite
            };
            
            positionImage.gameObject.SetActive(true);
        }
    }
}