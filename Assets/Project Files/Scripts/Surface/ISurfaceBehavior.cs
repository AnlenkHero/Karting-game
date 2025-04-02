using Kart.Project_Files.Scripts.Controls;

namespace Kart.Project_Files.Scripts.Surface
{
    public interface ISurfaceBehavior
    {
        void ApplyBehavior(KartController kart, SurfaceType surface);
    }
}