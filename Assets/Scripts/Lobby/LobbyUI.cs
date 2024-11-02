using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.UI;

namespace Kart
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] Button createLobbyButton;
        [SerializeField] Button joinLobbyButton;
        [SerializeField] Button refreshButton;
        [SerializeField] SceneReference gameScene;
        [SerializeField] LobbyList lobbyList;

        void Awake()
        {
            createLobbyButton.onClick.AddListener(CreateGame);
            joinLobbyButton.onClick.AddListener(JoinGame);
            refreshButton.onClick.AddListener(lobbyList.RefreshLobbyList);
        }

        async void CreateGame()
        {
            SetUIInteractable(false);
            await Multiplayer.Instance.CreateLobby();
            SetUIInteractable(true);
            Loader.LoadNetwork(gameScene);
        }

        async void JoinGame()
        {
            SetUIInteractable(false);
            await Multiplayer.Instance.QuickJoinLobby();
            SetUIInteractable(true);
        }

        private void SetUIInteractable(bool state)
        {
            createLobbyButton.interactable = state;
            joinLobbyButton.interactable = state;
            lobbyList.Lobbies.ForEach(lobby => lobby.SetUIInteraction(state));
        }
    }
}