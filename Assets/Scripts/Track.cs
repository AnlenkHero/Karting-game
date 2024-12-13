using UnityEngine;

namespace Kart
{
    public class Track : MonoBehaviour
    {
        [SerializeField] private LapCheckpoint[] checkpoints;
        [SerializeField] private FinishLine finishLine;
    }
}