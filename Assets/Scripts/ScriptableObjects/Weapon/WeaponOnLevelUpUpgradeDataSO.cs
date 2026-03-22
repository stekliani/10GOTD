using UnityEngine;

/// <summary>
/// Defines the stat bonus applied to a weapon when the player selects it
/// from the level-up upgrade screen.
/// </summary>
[CreateAssetMenu(menuName = "Weapons/Weapon On LevelUp Upgrade Data")]
public class WeaponOnLevelUpUpgradeDataSO : ScriptableObject
{
    [Header("Level-Up Upgrade Values")]
    public int   WeaponLevel;
    public float Damage;
    public float Speed;
    public float Interval;
    public float SlowAmount;
    public float FreezeDuration;
    public int Amount;
    public float Lifetime;
    public float Health;
    public float Armor;
    public float HealingMultiplier;
}
