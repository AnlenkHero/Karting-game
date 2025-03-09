using UnityEngine;
using UnityEngine.InputSystem;

namespace Kart.UI
{    [CreateAssetMenu(fileName = "UI Input Reader", menuName = "Scriptable Object/UI Input Reader")]
    public class InputUI : ScriptableObject, PlayerInputActions.IUIActions
    {
        public bool Cancel => inputActions.UI.Cancel.ReadValue<float>() > 0;

        PlayerInputActions inputActions;
        
        void OnEnable() {
            if (inputActions == null) {
                inputActions = new PlayerInputActions();
                inputActions.UI.SetCallbacks(this);
            }
        }
        
        public void Enable() {
            inputActions.Enable();
        }


        public void OnNavigate(InputAction.CallbackContext context)
        {
            //
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            //throw new System.NotImplementedException();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            //throw new System.NotImplementedException();
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            //throw new System.NotImplementedException();
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            //throw new System.NotImplementedException();
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
            //throw new System.NotImplementedException();
        }

        public void OnMiddleClick(InputAction.CallbackContext context)
        {
            //throw new System.NotImplementedException();
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
           // throw new System.NotImplementedException();
        }

        public void OnTrackedDevicePosition(InputAction.CallbackContext context)
        {
           // throw new System.NotImplementedException();
        }

        public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
        {
           // throw new System.NotImplementedException();
        }
    }
}