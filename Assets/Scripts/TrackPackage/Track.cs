using System;
using System.Collections;
using System.Resources;
using Fusion;
using Kart.Fusion;
using Unity.Mathematics;
using UnityEngine;
using ResourceManager = Kart.Managers.ResourceManager;

namespace Kart.TrackPackage
{
    public class Track : NetworkBehaviour
    {
        [Header("Track Setup")] public TrackData trackData;
        public LapCheckpoint lapCheckpointPrefab;
        public FinishLine finishLinePrefab;

        [Header("Runtime References")] public LapCheckpoint[] checkpoints;
        public FinishLine finishLine;

        
        private IEnumerator Start()
        {
            GameManager.CurrentTrack = this;
            yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.Object != null);
            GameManager.Instance.RPC_PrepareForRace();
        }

        public void Initialize()
        {
            if (trackData == null)
            {
                Debug.LogError("TrackData is not assigned to Track.");
                return;
            }

            ClearExistingCheckpoints();

            checkpoints = new LapCheckpoint[trackData.checkpoints.Length];

            for (int i = 0; i < trackData.checkpoints.Length; i++)
            {
                var data = trackData.checkpoints[i];
                var checkpointObj = Instantiate(lapCheckpointPrefab, data.position, data.rotation, transform);
                checkpointObj.transform.localScale = data.scale;
                checkpointObj.index = data.index;
                checkpoints[i] = checkpointObj;
            }

            if (finishLinePrefab != null)
            {
                var finishObj = Instantiate(
                    finishLinePrefab,
                    trackData.finishLinePosition,
                    trackData.finishLineRotation,
                    transform
                );

                finishObj.transform.localScale = trackData.finishLineScale;
                finishLine = finishObj;
            }

            Debug.Log("Track initialized. Checkpoints and FinishLine have been spawned.");
        }

        public void SpawnPlayer(NetworkRunner runner, RoomPlayer player)
        {
            var index = RoomPlayer.Players.IndexOf(player);
    //        var point = trackData.spawnPoints[index];

            var prefabId = player.KartId;
            var prefab = ResourceManager.Instance.kartDefinitions[prefabId].prefab;

            // Spawn player
            var entity = runner.Spawn(
                prefab,
                Vector3.zero,
                quaternion.identity,
                player.Object.InputAuthority
            );
            
            player.GameState = RoomPlayer.EGameState.GameCutscene;
            player.Kart = entity;

            Debug.Log($"Spawning kart for {player.Username} as {entity.name}");
            entity.transform.name = $"Kart ({player.Username})";
        }
        
        private void ClearExistingCheckpoints()
        {
            var otherCheckpoint =
                FindObjectsByType<LapCheckpoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (otherCheckpoint != null)
            {
                foreach (var checkpoint in otherCheckpoint)
                {
                    if (checkpoint != null)
                        Destroy(checkpoint.gameObject);
                }
            }

            var otherFinishLine = FindFirstObjectByType<FinishLine>();
            
            if (otherFinishLine != null)
            {
                Destroy(otherFinishLine.gameObject);
            }
        }
    }
}