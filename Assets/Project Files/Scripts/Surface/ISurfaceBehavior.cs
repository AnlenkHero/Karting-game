using Kart.Controls;

namespace Kart.Surface
{
    public interface ISurfaceBehavior
    {
        void ApplyBehavior(KartController kart, SurfaceType surface);
    }
}