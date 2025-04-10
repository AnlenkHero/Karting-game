using System.Collections.Generic;
using Fusion;
using Kart.Project_Files.Scripts.Definitions;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Managers.Game
{
    public class RaceTrackListManager : NetworkBehaviour
    {
        [Networked] public int CurrentRaceCount { get; private set; }
        [Networked] public int CurrentTrackIndex { get; private set; }
        public const int MaxRaces = 5;
        public TrackDefinition CurrentTrackDefinition;
        private List<TrackDefinition> availableTracks;


        public override void Spawned()
        {
            base.Spawned();
            availableTracks = new List<TrackDefinition>(ResourceManager.Instance.tracks);
        }


        public void RemoveTrack(TrackDefinition track)
        {
            if (track != null)
            {
                availableTracks.Remove(track);
            }
        }

        public void AdvanceToNextRaceTrack()
        {
            CurrentRaceCount++;

            if (availableTracks is { Count: > 0 })
            {
                int randomIndex = Random.Range(0, availableTracks.Count);
                CurrentTrackIndex = randomIndex;
                Debug.Log("current index " + randomIndex);
                CurrentTrackDefinition = availableTracks[randomIndex];
                availableTracks.RemoveAt(randomIndex);
            }
            else
            {
                Debug.LogWarning("No more available tracks. Reinitializing track list.");
                // Optionally, reinitialize availableTracks here if you want to allow repeats in a longer session.
            }
        }
    }
}