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
            DrawDefaultInspector();
            GUILayout.Space(10);

            TrackSaver trackSaver = (TrackSaver)target;

            if (GUILayout.Button("Save Track"))
            {
                trackSaver.SaveTrack();

                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(trackSaver.trackData);
                    // Uncomment the following line if you want to automatically save the scene
                    // EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
        }
    }
}
#endif