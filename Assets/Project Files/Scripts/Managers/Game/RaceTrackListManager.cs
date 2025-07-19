using System.Collections.Generic;
using Fusion;
using Kart.Project_Files.Scripts.Definitions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kart.Project_Files.Scripts.Managers.Game
{
    public class RaceTrackListManager : NetworkBehaviour
    {
        [Networked] public int CurrentRaceCount { get; private set; }
        [Networked] public int CurrentTrackIndex { get; private set; }
        public TrackDefinition currentTrackDefinition;
        private List<TrackDefinition> _availableTracks;


        public override void Spawned()
        {
            base.Spawned();
            _availableTracks = new List<TrackDefinition>(ResourceManager.Instance.tracks);
        }


        public void RemoveTrack(TrackDefinition track)
        {
            if (track != null)
            {
                _availableTracks.Remove(track);
            }
        }

        public void AdvanceToNextRaceTrack()
        {
            CurrentRaceCount++;

            if (_availableTracks.Count > 0)
            {
                int randomIndex = Random.Range(0, _availableTracks.Count);
                CurrentTrackIndex = randomIndex;
                currentTrackDefinition = _availableTracks[randomIndex];
                _availableTracks.RemoveAt(randomIndex);
                Debug.Log("current index " + randomIndex);
            }
            else
            {
                Debug.LogWarning("No more available tracks.");
            }
        }
    }
}