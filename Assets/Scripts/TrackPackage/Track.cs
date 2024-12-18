using System;
using UnityEngine;

namespace Kart.TrackPackage
{
    public class Track : MonoBehaviour
    {
        [Header("Track Setup")] 
        public TrackData trackData;
        public LapCheckpoint lapCheckpointPrefab;
        public FinishLine finishLinePrefab;

        [Header("Runtime References")] 
        public LapCheckpoint[] checkpoints; 
        public FinishLine finishLine; 

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Instantiates and enumerates checkpoints & finish line from TrackData.
        /// </summary>
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