using UnityEngine;
using System;

public enum SoundActions
{
    none,
    playArrowLaunch,
    playArrowHit,
    playRocketLaunch,
    playRocketHit,
    playClick,
    playOnLevelUp,
}

public static class SoundEventBus
{
    // Event that broadcasts a sound action
    public static event Action<SoundActions> OnSoundEvent;

    public static void Raise(SoundActions action)
    {
        OnSoundEvent?.Invoke(action);
    }
}