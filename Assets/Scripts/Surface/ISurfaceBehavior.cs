using Kart.Controls;

namespace Kart.Surface
{
    public interface ISurfaceBehavior
    {
        bool IsContinuous { get; set; }
        void ApplyBehavior(KartController kart, SurfaceType surface);
    }
}