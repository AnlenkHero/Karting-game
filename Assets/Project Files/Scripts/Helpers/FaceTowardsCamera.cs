using UnityEngine;

namespace Kart.Project_Files.Scripts.Helpers
{
    public class FaceTowardsCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Camera.main != null) 
                transform.rotation = Camera.main.transform.rotation;
        }
    }
}