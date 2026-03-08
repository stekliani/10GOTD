using UnityEngine;

public class SlowingWeapon : Weapon
{
    protected override void Fire() { /* Aura — no fire event */ }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out ISlowable slowable))
            slowable.ApplySlow(data.SlowAmount);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out ISlowable slowable))
            slowable.RemoveSlow(data.SlowAmount);
    }
}
