using UnityEngine;

namespace Kart.Project_Files.Scripts.Controls
{
    public interface IDrive
    {
        Vector2 Move { get; }
        bool IsBraking { get; }
        void Enable();
    }
}