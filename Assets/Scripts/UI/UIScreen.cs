using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Needed for EventSystem

namespace Kart.UI
{
    public class UIScreen : MonoBehaviour
    {
        
        public bool isModal = false;
        [SerializeField] private Selectable firstSelected = null;
        public UIScreen previousScreen = null;

        public static UIScreen activeScreen;
        public static UIScreen rootScreen;

        public static void SetRootScreen(UIScreen screen)
        {
            rootScreen = screen;
        }
        public static void Focus(UIScreen screen)
        {
            if (screen == activeScreen)
                return;

            if (activeScreen)
                activeScreen.Defocus();
            screen.previousScreen = activeScreen;
            activeScreen = screen;
            screen.Focus();
            Debug.Log(activeScreen);
        }

        public static void BackToInitial()
        {
            activeScreen?.BackTo(null);
        }
        
        public void FocusScreen(UIScreen screen)
        {
            Focus(screen);
        }

        private void Focus()
        {
            if (!gameObject) return;
            
            gameObject.SetActive(true);
                
            if (firstSelected != null)
            {
                firstSelected.Select();
            }
        }

        private void Defocus()
        {
            if (gameObject)
                gameObject.SetActive(false);
        }

        public void Back()
        {
            if (!previousScreen)
            {
                Defocus();
                return;
            }

            Defocus();
            activeScreen = previousScreen;
            activeScreen.Focus();
            previousScreen = null;
            Debug.Log(activeScreen);
        }

        public void BackTo(UIScreen screen)
        {
            while (activeScreen != null && activeScreen.previousScreen != null && activeScreen != screen)
                activeScreen.Back();
        }
    }
}
