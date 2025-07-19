using System.Collections;
using Fusion;
using Kart.Project_Files.Scripts.Fusion;
using Kart.Project_Files.Scripts.Helpers;
using Kart.Project_Files.Scripts.Managers.Game;
using Kart.Project_Files.Scripts.Managers.Interface;
using Kart.Project_Files.Scripts.UI.Animations;
using Kart.Project_Files.Scripts.UI.Screens;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Managers
{
    public class LevelManager : NetworkSceneManagerDefault
    {
        public const int MAIN_MENU_SCENE = 1;
        public const int RESULTS_SCENE = 6;

        [SerializeField] private UIScreen _dummyScreen;
        [SerializeField] private UIScreen _lobbyScreen;
        [SerializeField] private CanvasFader fader;

        public static LevelManager Instance => Singleton<LevelManager>.Instance;

        public static void LoadSceneByIndex(int sceneIndex)
        {
            Instance.Runner.LoadScene(SceneRef.FromIndex(sceneIndex));
        }

        protected override IEnumerator LoadSceneCoroutine(SceneRef sceneRef, NetworkLoadSceneParameters sceneParams)
        {
            Debug.Log($"Loading scene {sceneRef}");
            PreLoadScene();
            yield return base.LoadSceneCoroutine(sceneRef, sceneParams);
            
            yield return null;
            
            PostLoadScene();

            if (GameManager.Instance.currentTrack == null || sceneRef.AsIndex <= MAIN_MENU_SCENE ||
                GameManager.Instance.CurrentGameState >= GameState.Running) yield break;
            if (!Runner.IsServer) yield break;
            
            foreach (var player in RoomPlayer.Players)
            {
                player.GameState = RoomPlayer.EGameState.GameCutscene;
                GameManager.Instance.currentTrack.SpawnPlayer(Runner, player);
            }
        }

        private void PreLoadScene()
        {
            fader.gameObject.SetActive(true);
            fader.FadeIn();
        }

        private void PostLoadScene()
        {
            fader.FadeOut();
        }
        
        private void ShowLoadingScreen(bool state)
        {
            InterfaceManager.Instance.ShowLoadingScreen(state);
        }
    }
}