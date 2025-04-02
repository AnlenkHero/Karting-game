using Kart.Project_Files.Scripts.Controls;
using UnityEngine;

namespace Kart.Project_Files.Scripts.Surface
{
    public abstract class SurfaceBehavior : ScriptableObject, ISurfaceBehavior
    {
        public abstract void ApplyBehavior(KartController kart, SurfaceType surface);
    }

}