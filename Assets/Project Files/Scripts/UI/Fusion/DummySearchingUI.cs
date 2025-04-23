using TMPro;
using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Fusion
{
    public class DummySearchingUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;

        private float _elapsedTime;
        private bool _isSearching;
        
        public void StartSearching()
        {
            _elapsedTime = 0f;
            _isSearching = true;
        }


        public void StopSearching()
        {
            _isSearching = false;
        }

        private void Update()
        {
            if (!_isSearching) return;
            
            _elapsedTime += Time.deltaTime;
            timerText.text = "Searching... " + Mathf.FloorToInt(_elapsedTime) + "s";
        }
    }
}