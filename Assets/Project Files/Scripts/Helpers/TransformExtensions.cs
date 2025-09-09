using UnityEngine;

namespace Kart.Project_Files.Scripts.Helpers
{
    public static class TransformExtensions
    {
        public static void ClearExistingElementsInParent(this Transform parentTransform)
        {
            foreach (Transform child in parentTransform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}