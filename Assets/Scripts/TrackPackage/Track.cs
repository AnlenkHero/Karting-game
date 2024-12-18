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
        public LapCheckpoint[] checkpoints;  // actual checkpoint GameObjects in the scene
        public FinishLine finishLine;        // actual finish line GameObject in the scene

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

            // Clean up any existing references first (if re-initializing)
            ClearExistingCheckpoints();

            // Instantiate checkpoints from TrackData
            checkpoints = new LapCheckpoint[trackData.checkpoints.Length];
            for (int i = 0; i < trackData.checkpoints.Length; i++)
            {
                var data = trackData.checkpoints[i];
                var checkpointObj = Instantiate(lapCheckpointPrefab, data.position, data.rotation, transform);
                checkpointObj.transform.localScale = data.scale;
                checkpointObj.index = data.index;  // Assign the index
                checkpoints[i] = checkpointObj;
            }

            // Instantiate finish line
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
            // Destroy old checkpoint objects if they exist
            if (checkpoints != null)
            {
                foreach (var checkpoint in checkpoints)
                {
                    if (checkpoint != null)
                        DestroyImmediate(checkpoint.gameObject);
                }
            }
            // Destroy old finish line if it exists
            if (finishLine != null)
            {
                DestroyImmediate(finishLine.gameObject);
            }
        }
    }
}
