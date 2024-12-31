using UnityEngine;

namespace Kart.Controls
{
    public interface IDrive
    {
        Vector2 Move { get; }
        bool IsBraking { get; }
        void Enable();
    }
}