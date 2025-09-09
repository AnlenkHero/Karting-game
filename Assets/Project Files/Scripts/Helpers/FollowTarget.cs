using UnityEngine;

namespace Kart.Project_Files.Scripts.Helpers
{
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;

        private void Update()
        {
            if (target)
            {
                transform.position = target.position;
            }
        }
    }
}