using UnityEngine;

namespace Kart
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