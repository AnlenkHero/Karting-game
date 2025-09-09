using TMPro;
using UnityEngine;

namespace Kart.Project_Files.Scripts.UI.Animations.Button
{
    public class DisabledButtonText : MonoBehaviour
    {
        [SerializeField] UnityEngine.UI.Button button;
        [SerializeField] TextMeshProUGUI textTarget;
        [SerializeField] TMP_FontAsset normalFont;
        [SerializeField] TMP_FontAsset disabledFont;

        bool _lastState;

        void Awake()
        {
            _lastState = button.interactable;
            ApplyFont(_lastState);
        }

        void Update()
        {
            if (button.interactable == _lastState) return;
            _lastState = button.interactable;
            ApplyFont(_lastState);
        }

        void ApplyFont(bool interactable)
            => textTarget.font = interactable ? normalFont : disabledFont;
    }
}