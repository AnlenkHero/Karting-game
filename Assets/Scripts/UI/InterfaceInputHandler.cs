using UnityEngine;
using UnityEngine.InputSystem;


namespace Kart.UI
{
    public class InterfaceInputHandler : MonoBehaviour, PlayerInputActions.IUIActions
    {
        [SerializeField] private InterfaceScreenHandler screenHandler;
        private PlayerInputActions inputActions;

        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }

        private void Awake()
        {
            inputActions = new PlayerInputActions();
            inputActions.UI.SetCallbacks(this);

            if (screenHandler == null)
            {
                Debug.LogError("InterfaceScreenHandler not found for MergedInputHandler");
            }
        }


        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed)
                return;

            if (GameManager.Instance != null && GameManager.Instance.CurrentGameState == GameState.Running &&
                InterfaceManager.Instance.ActiveScreen == InterfaceManager.Instance.RootScreen)
            {
                screenHandler.ShowScreen(screenHandler.EscapeMenuScreen);
            }
            else
            {
                if (screenHandler.ActiveScreen != screenHandler.RootScreen)
                {
                    screenHandler.CloseToRoot();
                }
            }
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
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