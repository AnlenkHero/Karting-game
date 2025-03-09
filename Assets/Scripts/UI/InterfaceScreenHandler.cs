using System.Collections.Generic;
using UnityEngine;

namespace Kart.UI
{
    public class InterfaceScreenHandler : MonoBehaviour
    {
        private UIScreen rootScreen;
        [SerializeField] private InterfaceGlobalScreenStorage globalScreenStorage;
        [SerializeField] private UIScreen dummyRootScreen;

        private readonly Stack<UIScreen> screenStack = new();


        public UIScreen RootScreen => rootScreen != null ? rootScreen : SetRootScreen(dummyRootScreen);

        public UIScreen EscapeMenuScreen => globalScreenStorage.escapeMenu;

        public UIScreen SettingsMenuScreen => globalScreenStorage.settingsMenu;

        public UIScreen ActiveScreen => screenStack.Count > 0 ? screenStack.Peek() : null;

        private void Awake()
        {
            if (rootScreen == null) return;

            screenStack.Push(rootScreen);
            rootScreen.Show();
        }

        public UIScreen SetRootScreen(UIScreen rootScreen)
        {
            var screen = rootScreen;
            if (rootScreen == null)
            {
                screen = dummyRootScreen;
            }

            this.rootScreen = screen;
            screenStack.Clear();
            screenStack.Push(screen);
            screen.Show();
            return screen;
        }

        public void ShowScreen(UIScreen screen)
        {
            if (screen == null)
                return;

            if (ActiveScreen == screen)
                return;

            if (screenStack.Contains(screen))
            {
                while (ActiveScreen != screen)
                {
                    UIScreen top = screenStack.Pop();
                    top.Hide();
                }

                ActiveScreen.Show();
            }
            else
            {
                if (ActiveScreen != null)
                    ActiveScreen.Hide();

                screenStack.Push(screen);
                screen.Show();
            }
        }

        public void CloseActiveScreen()
        {
            if (screenStack.Count <= 1) return;
            
            UIScreen top = screenStack.Pop();
            top.Hide();
            ActiveScreen.Show();
        }
        
        public void CloseToRoot()
        {
            while (screenStack.Count > 1)
            {
                UIScreen top = screenStack.Pop();
                top.Hide();
            }

            if (screenStack.Count > 0)
                screenStack.Peek().Show();
        }
    }
}