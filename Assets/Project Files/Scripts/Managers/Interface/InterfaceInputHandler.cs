using Kart.Project_Files.Scripts.Controls;
using Kart.Project_Files.Scripts.Managers.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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


        public void OnEscape(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) return;

            var kart = KartController.LocalKartController;
            var gameState = GameManager.Instance?.CurrentGameState;
            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            bool isRoot = interfaceManager.ActiveScreen == interfaceManager.RootScreen;

            if (kart != null)
                kart.canDrive = false;

            if (gameState is >= GameState.PreGame and < GameState.Finished && isRoot &&
                currentSceneIndex > LevelManager.MAIN_MENU_SCENE)
            {
                interfaceManager.ShowScreen(interfaceManager.EscapeMenuScreen);
                return;
            }

            if (isRoot) return;

            interfaceManager.CloseToRoot();

            if (kart != null && gameState >= GameState.Running)
                kart.canDrive = true;
        }

        public void OnBack(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed) return;
            if (interfaceManager.ActiveScreen == interfaceManager.RootScreen) return;
            if (EventSystem.current.currentSelectedGameObject.GetComponentInParent<TMP_Dropdown>() &&
                !EventSystem.current.currentSelectedGameObject.GetComponent<TMP_Dropdown>()) return;

            interfaceManager.CloseActiveScreen();

            var kart = KartController.LocalKartController;
            bool isNowRoot = interfaceManager.ActiveScreen == interfaceManager.RootScreen;
            var gameState = GameManager.Instance?.CurrentGameState;

            if (kart != null && isNowRoot && gameState >= GameState.Running)
                kart.canDrive = true;
        }

        public void OnNavigate(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed)
                return;

            navigationInput = context.ReadValue<Vector2>();
        }

        #region Unused Interface Methods

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

        #endregion
    }
}