using System.Collections.Generic;
using UnityEngine;

public abstract class Subject : MonoBehaviour
{
    private readonly List<IInputObserver> _observers = new();
    public void AddObserver(IInputObserver observer)    => _observers.Add(observer);
    public void RemoveObserver(IInputObserver observer) => _observers.Remove(observer);

    protected void NotifyObservers(InputActions action)
    {
        foreach (IInputObserver o in _observers)
            o.OnNotify(action);
    }
}
