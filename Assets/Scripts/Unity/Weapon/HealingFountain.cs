using UnityEngine;

public class HealingFountain : Weapon
{

    protected new void Awake()
    {
        base.Awake();
        IsActive = false;
    }

    protected override void Fire() { /* Passive — no fire action */ }

    public float GetHealingAmountMultiplier()
    {
        return IsActive
            ? data.HealingMultiplier
            : 1f;
    }
}
