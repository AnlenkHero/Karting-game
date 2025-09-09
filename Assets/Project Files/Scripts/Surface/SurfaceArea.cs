using UnityEngine;

namespace Kart.Project_Files.Scripts.Surface
{
    [RequireComponent(typeof(Collider))]
    public class SurfaceArea : MonoBehaviour
    {
        public int priority;
        public SurfaceType surface;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }
    }
}