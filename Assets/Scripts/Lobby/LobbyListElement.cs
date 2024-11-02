using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kart
{
    public class LobbyListElement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI lobbyText;
        [SerializeField] private Button joinButton;

        public void SetupLobbyListElement(string lobbyInfo, Action joinAction)
        {
            lobbyText.text = lobbyInfo;
            joinButton.onClick.AddListener(() => joinAction());
        }

        public void SetUIInteraction(bool state)
        {
            joinButton.interactable = state;
        }
    }
}