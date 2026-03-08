using System;
using UnityEngine;

public abstract class AnimationSubject : MonoBehaviour
{
    public static event Action<AnimationActions, Vector3> OnAnimationGlobal;

    protected void NotifyObservers(AnimationActions action, Vector3 position)
    {
        OnAnimationGlobal?.Invoke(action, position);
    }
}
