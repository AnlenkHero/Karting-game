using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.Settings;
using UnityEngine;
using TMPro;

namespace Kart.Project_Files.Scripts.UI.GeneralGame
{
    public class RaceStartUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countdownText;
        [SerializeField] private float postStartDisplay = 1f;
        [SerializeField] private GameObject minimap;
        [SerializeField] private float countdownLength = 3f; 
        private GameManager _gameManager;
        private bool _isCounting;

        private void Start()
        {
            _gameManager = GameManager.Instance;
        }

        private void Update()
        {
            if (_gameManager.CurrentGameState != GameState.PreGame)
            {
                if (!_isCounting) return;
                
                _isCounting = false;
                gameObject.SetActive(false);
                return;
            }

            float remaining = GameConfig.RaceStartDelay - _gameManager.ElapsedTime;
            
            if (remaining <= countdownLength && remaining > 0f)
            {
                if (!_isCounting)
                {
                    _isCounting = true;
                    gameObject.SetActive(true);
                    countdownText.gameObject.SetActive(true);
                }

                countdownText.text = Mathf.CeilToInt(remaining).ToString();
            }
            else if (_isCounting)
            {
                _isCounting = false;
                gameObject.SetActive(false);
            }
        }
    }
}