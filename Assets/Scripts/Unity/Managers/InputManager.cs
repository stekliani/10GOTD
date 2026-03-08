using UnityEngine;

public class InputManager : Subject
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) NotifyObservers(InputActions.OpenStatsWindow);
        if (Input.GetKeyUp(KeyCode.E))        NotifyObservers(InputActions.OpenLevelUpWindow);
    }
}
