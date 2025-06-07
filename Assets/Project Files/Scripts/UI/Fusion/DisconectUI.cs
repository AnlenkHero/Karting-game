using System;
using Kart.Project_Files.Scripts.Managers.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
            closeButton.onClick.AddListener(HideMessage);
        }

        public void Update()
        {
            EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
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
        
        public void HideMessage()
        {
            Debug.Log("Hiding message");
            parent.gameObject.SetActive(false);
            InterfaceManager.Instance.CloseToRoot();
        }
    }
}