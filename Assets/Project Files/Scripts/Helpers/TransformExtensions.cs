using UnityEngine;

namespace Kart.Helpers
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