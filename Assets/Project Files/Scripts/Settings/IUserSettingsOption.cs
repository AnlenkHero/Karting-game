using System;

namespace Kart.Project_Files.Scripts.Settings
{
    public interface IUserSettingsOption
    {
        bool IsValid();
        void ApplySetting();
        event Action OnValidityChanged;
    }
}