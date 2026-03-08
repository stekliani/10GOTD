using System;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    public static EventsManager Instance { get; private set; }

    
    public event EventHandler  OnLastEnemyDeath;


    public void FireOnLastEnemyDeathEvent()
    {
        OnLastEnemyDeath?.Invoke(this, EventArgs.Empty);
    }
}
