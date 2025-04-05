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
        public const int LAUNCH_SCENE = 0;
        public const int MAIN_MENU_SCENE = 1;

        [SerializeField] private UIScreen _dummyScreen;
        [SerializeField] private UIScreen _lobbyScreen;
        [SerializeField] private CanvasFader fader;

        public static LevelManager Instance => Singleton<LevelManager>.Instance;

        public static void LoadMenu()
        {
            Instance.Runner.LoadScene(SceneRef.FromIndex(MAIN_MENU_SCENE));
        }

        public static void LoadTrack(int sceneIndex)
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
            
            if (GameManager.CurrentTrack != null && sceneRef.AsIndex > MAIN_MENU_SCENE && GameManager.Instance.CurrentGameState < GameState.Running)
            {
                if (Runner.IsServer)
                {
                    foreach (var player in RoomPlayer.Players)
                    {
                        player.GameState = RoomPlayer.EGameState.GameCutscene;
                        GameManager.CurrentTrack.SpawnPlayer(Runner, player);
                    }
                }
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