using System.Collections.Generic;
using UnityEngine;

namespace Kart.UI
{
    public class InterfaceScreenHandler : MonoBehaviour
    {
        private UIScreen _rootScreen;
        [SerializeField] private InterfaceGlobalScreenStorage globalScreenStorage;
        [SerializeField] private UIScreen dummyRootScreen;

        private readonly Stack<UIScreen> _screenStack = new();
        
        public UIScreen RootScreen => _rootScreen != null ? _rootScreen : SetRootScreen(dummyRootScreen);

        public UIScreen EscapeMenuScreen => globalScreenStorage.escapeMenu;

        public UIScreen SettingsMenuScreen => globalScreenStorage.settingsMenu;

        public UIScreen ActiveScreen => _screenStack.Count > 0 ? _screenStack.Peek() : null;

        private void Awake()
        {
            if (_rootScreen == null) return;

            _screenStack.Push(_rootScreen);
            _rootScreen.Show();
        }

        public UIScreen SetRootScreen(UIScreen rootScreen)
        {
            var screen = rootScreen;
            if (rootScreen == null)
            {
                screen = dummyRootScreen;
            }

            _rootScreen = screen;
            _screenStack.Clear();
            _screenStack.Push(screen);
            screen.Show();
            return screen;
        }

        public void ShowScreen(UIScreen screen)
        {
            if (screen == null)
                return;

            if (ActiveScreen == screen)
                return;

            if (_screenStack.Contains(screen))
            {
                while (ActiveScreen != screen)
                {
                    UIScreen top = _screenStack.Pop();
                    top.Hide();
                }

                ActiveScreen.Show();
            }
            else
            {
                if (ActiveScreen != null)
                    ActiveScreen.Hide();

                _screenStack.Push(screen);
                screen.Show();
            }
        }

        public void CloseActiveScreen()
        {
            if (_screenStack.Count <= 1) return;
            
            UIScreen top = _screenStack.Pop();
            top.Hide();
            ActiveScreen.Show();
        }
        
        public void CloseToRoot()
        {
            while (_screenStack.Count > 1)
            {
                UIScreen top = _screenStack.Pop();
                top.Hide();
            }

            if (_screenStack.Count > 0)
                _screenStack.Peek().Show();
        }
    }
}