using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Project_Files.Scripts.UI.Fusion
{
    public class DisconnectUI : MonoBehaviour
    {
        [SerializeField] private Transform parent;
        [SerializeField] private TextMeshProUGUI disconnectStatus;
        [SerializeField] private TextMeshProUGUI disconnectMessage;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            closeButton.onClick.AddListener(() => parent.gameObject.SetActive(false));
        }

        public void ShowMessage(string status, string message)
        {
            if (status == null || message == null)
                return;

            disconnectStatus.text = status;
            disconnectMessage.text = message;

            Debug.Log($"Showing message({status},{message})");
            parent.gameObject.SetActive(true);
        }
    }
}