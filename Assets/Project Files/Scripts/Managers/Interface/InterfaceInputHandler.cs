using Kart.Project_Files.Scripts.Managers.Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kart.Project_Files.Scripts.Managers.Interface
{
    public class InterfaceInputHandler : MonoBehaviour, PlayerInputActions.IUIActions
    {
        [SerializeField] private InterfaceManager interfaceManager;
        private PlayerInputActions _inputActions;
        public Vector2 navigationInput;

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.UI.SetCallbacks(this);
        }


        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed)
                return;

            if (GameManager.Instance != null && GameManager.Instance.CurrentGameState >= GameState.Cutscene &&
                interfaceManager.ActiveScreen == interfaceManager.RootScreen)
            {
                interfaceManager.ShowScreen(interfaceManager.EscapeMenuScreen);
            }
            else
            {
                if (interfaceManager.ActiveScreen != interfaceManager.RootScreen)
                {
                    interfaceManager.CloseToRoot();
                }
            }
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed)
                return;

            navigationInput = context.ReadValue<Vector2>();
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
        }

        public void OnClick(InputAction.CallbackContext context)
        {
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
        }

        public void OnTrackedDevicePosition(InputAction.CallbackContext context)
        {
        }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
        {
        }
    }
}