using Kart.Controls;
using UnityEngine;

namespace Kart.Surface
{
    public abstract class SurfaceBehavior : ScriptableObject, ISurfaceBehavior
    {
        public abstract bool IsContinuous { get; set; }
        public abstract void ApplyBehavior(KartController kart, SurfaceType surface);
    }

}