using UnityEngine;

/// <summary>
/// Defines the incremental stat bonus applied when the player spends coins
/// to upgrade a weapon at runtime (upgrade menu).
/// </summary>
[CreateAssetMenu(menuName = "Weapons/Weapon Upgrade Data")]
public class WeaponUpgradeDataSO : ScriptableObject
{
    [Header("Runtime Upgrade Values")]
    [HideInInspector] public int   WeaponLevel;
    public int   UpgradeLevel;
    public float Damage;
    public float Speed;
    public float Interval;
    public float SlowAmount;
    public float Lifetime;
    public float Health;
    public float Armor;
    public float HealingMultiplier;
}
