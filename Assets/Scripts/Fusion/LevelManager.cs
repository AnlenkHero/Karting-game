using System.Collections;
using Fusion;
using Kart;
using Kart.Fusion;
using Kart.Helpers;
using Kart.UI;
using UnityEngine;


namespace Managers
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

	
			
			yield return base.LoadSceneCoroutine(sceneRef, sceneParams);
			
			// Delay one frame, so we're sure level objects has spawned locally
			yield return null;
			
			// Now we can safely spawn karts
			if (GameManager.CurrentTrack != null && sceneRef.AsIndex > MAIN_MENU_SCENE)
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


		}


	}
}