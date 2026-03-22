using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Subject
{
    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            NotifyObservers(InputActions.OpenStatsWindow);
        }

        if (keyboard.eKey.wasReleasedThisFrame)
        {
            NotifyObservers(InputActions.OpenLevelUpWindow);
        }
    }
}
