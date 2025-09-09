using Fusion;
using Kart.Project_Files.Scripts.Controls;
using UnityEngine;

namespace Kart.Project_Files.Scripts.TrackPackage
{
    public class Deadzone : NetworkBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<KartResetter>()) return;
            var resetter = other.GetComponent<KartResetter>(); 
            if (Object.HasStateAuthority)
            {
                Debug.Log("Deadzone Triggered");
                resetter.ForceRespawn();
            }
        }
    }
}