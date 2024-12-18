#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kart.TrackPackage
{
    public class TrackSaver : MonoBehaviour
    {
        public TrackData trackData; // The ScriptableObject to save into

        [ContextMenu("Save Track to TrackData")]
        public void SaveTrack()
        {
            if (!trackData)
            {
                Debug.LogError("No TrackData assigned!");
                return;
            }

            // Gather all LapCheckpoint objects in the scene
            var lapCheckpoints = FindObjectsOfType<LapCheckpoint>();
            trackData.checkpoints = new TrackData.CheckpointData[lapCheckpoints.Length];
            for (int i = 0; i < lapCheckpoints.Length; i++)
            {
                var cp = lapCheckpoints[i];
                trackData.checkpoints[i] = new TrackData.CheckpointData
                {
                    index = cp.index,
                    position = cp.transform.position,
                    rotation = cp.transform.rotation,
                    scale = cp.transform.localScale
                };
            }

            // Gather FinishLine
            var finish = FindObjectOfType<FinishLine>();
            if (finish != null)
            {
                trackData.finishLinePosition = finish.transform.position;
                trackData.finishLineRotation = finish.transform.rotation;
                trackData.finishLineScale = finish.transform.localScale;
            }

            EditorUtility.SetDirty(trackData);
            Debug.Log("Track data saved to ScriptableObject with position, rotation, and scale!");
        }
    }
}
#endif