using UnityEngine;

namespace Kart.Project_Files.Scripts.TrackPackage
{
    [CreateAssetMenu(fileName = "TrackData", menuName = "Kart/TrackData")]
    public class TrackData : ScriptableObject
    {
        [System.Serializable]
        public class CheckpointData
        {
            public int index;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }
        
        [System.Serializable]
        public class SpawnPointData
        {
            public int index;
            public Vector3 position;
            public Quaternion rotation;
        }
        
        public SpawnPointData[] spawnPoints; 
        public CheckpointData[] checkpoints;
        public Vector3 finishLinePosition;
        public Quaternion finishLineRotation;
        public Vector3 finishLineScale = Vector3.one;
    }
}