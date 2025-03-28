using System.Collections;
using Fusion;
using Kart.Helpers;
using Kart.Managers;
using Kart.UI;
using UnityEngine;

namespace Kart.Fusion
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
            ShowLoadingScreen(true);


            yield return base.LoadSceneCoroutine(sceneRef, sceneParams);
            
            yield return null;
            
            if (GameManager.CurrentTrack != null && sceneRef.AsIndex > MAIN_MENU_SCENE && GameManager.Instance.CurrentGameState < GameState.Running)
            {
                if (Runner.GameMode == GameMode.Host)
                {
                    foreach (var player in RoomPlayer.Players)
                    {
                        player.GameState = RoomPlayer.EGameState.GameCutscene;
                        GameManager.CurrentTrack.SpawnPlayer(Runner, player);
                    }
                }
            }
            
            ShowLoadingScreen(false);
        }

        private void PreLoadScene(int scene)
        {
            /*if (scene > MAIN_MENU_SCENE)
            {
                // Show an empty dummy UI screen - this will stay on during the game so that the game has a place in the navigation stack. Without this, Back() will break
                Debug.Log("Showing Dummy");
                UIScreen.Focus(_dummyScreen);
            }
            else if(scene==MAIN_MENU_SCENE)
            {
                foreach (RoomPlayer player in RoomPlayer.Players)
                {
                    player.IsReady = false;
                }
                UIScreen.activeScreen.BackTo(_lobbyScreen);
            }
            else
            {
                UIScreen.BackToInitial();
            }
            fader.gameObject.SetActive(true);
            fader.FadeIn();*/
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