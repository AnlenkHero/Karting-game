using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if (ENABLE_INPUT_SYSTEM)
using UnityEngine.InputSystem;
#endif

namespace UniStorm.CharacterController
{
    public class Pause : MonoBehaviour
    {
        bool Paused = false;

        void Update()
        {
#if (ENABLE_LEGACY_INPUT_MANAGER)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Paused = !Paused;
            }
#elif (ENABLE_INPUT_SYSTEM)
            if (Keyboard.current.escapeKey.isPressed)
            {
                Paused = !Paused;
            }
#endif

            if (Paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                GetComponent<UniStormMouseLook>().enabled = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                GetComponent<UniStormMouseLook>().enabled = true;
            }
        }
    }
}