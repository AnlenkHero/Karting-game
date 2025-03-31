using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.UI
{
    public class DummySearchingUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;

        private float elapsedTime = 0f;
        private bool isSearching = false;
        
        public void StartSearching()
        {
            elapsedTime = 0f;
            isSearching = true;
        }


        public void StopSearching()
        {
            isSearching = false;
        }

        private void Update()
        {
            if (!isSearching) return;
            
            elapsedTime += Time.deltaTime;
            timerText.text = "Searching... " + Mathf.FloorToInt(elapsedTime).ToString() + "s";
        }
    }
}