#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kart.TrackPackage
{
    [CustomEditor(typeof(TrackSaver))]
    public class TrackSaverEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Add some space
            GUILayout.Space(10);

            // Reference to the target script
            TrackSaver trackSaver = (TrackSaver)target;

            // Add a button labeled "Save Track"
            if (GUILayout.Button("Save Track"))
            {
                // Call the SaveTrack method when the button is clicked
                trackSaver.SaveTrack();

                // Optionally, mark the scene as dirty to ensure changes are saved
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(trackSaver.trackData);
                    // Save the scene if needed
                    // Uncomment the following line if you want to automatically save the scene
                    // EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
        }
    }
}
#endif