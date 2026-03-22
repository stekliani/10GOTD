using System;
using UnityEngine;

/// <summary>
/// Data container for a weapon type. Runtime state is stored on the INSTANCE copy
/// created at spawn time (via Instantiate) so ScriptableObject assets are never mutated.
/// </summary>
[CreateAssetMenu(menuName = "Weapons/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [Header("Info")]
    public Sprite weaponSprite;
    public string weaponName;

    [SerializeField] private int   baseWeaponLevel;
    [SerializeField] private int   baseWeaponUpgradeCost;
    [SerializeField] private int   baseUpgradeCostPerLevel;
    [SerializeField] private int   baseUpgradeLevel;

    [Header("Offense")]
    [SerializeField] private float baseDamage;
    [SerializeField] private float baseSpeed;
    [SerializeField] private float baseInterval;
    [SerializeField] private float baseSlowAmount;
    [SerializeField] private float baseFreezeDuration;
    [SerializeField] private int baseAmount;

    [Header("Lifetime")]
    [SerializeField] private float baseLifetime;

    [Header("Defense")]
    [SerializeField] private float baseHealth;
    [SerializeField] private float baseArmor;
    [SerializeField] private float baseHealingMultiplier;

    // ── Runtime values (on instance copy) ─────────────────────────────────────
    [HideInInspector] public int   WeaponLevel;
    [HideInInspector] public int   UpgradeLevel;
    [HideInInspector] public int   WeaponUpgradeCost;
    [HideInInspector] public int   UpgradeCostPerLevel;
    [HideInInspector] public int Amount;
    [HideInInspector] public float Damage;
    [HideInInspector] public float Speed;
    [HideInInspector] public float Interval;
    [HideInInspector] public float Area;
    [HideInInspector] public float SlowAmount;
    [HideInInspector] public float FreezeDuration;
    [HideInInspector] public float Lifetime;
    [HideInInspector] public float Health;
    [HideInInspector] public float Armor;
    [HideInInspector] public float HealingMultiplier;

    public void InitializeWeaponDefaults()
    {
        WeaponLevel        = baseWeaponLevel;
        UpgradeLevel       = baseUpgradeLevel;
        WeaponUpgradeCost  = baseWeaponUpgradeCost;
        UpgradeCostPerLevel= baseUpgradeCostPerLevel;
        Damage             = baseDamage;
        Speed              = baseSpeed;
        Interval           = baseInterval;
        SlowAmount         = baseSlowAmount;
        FreezeDuration     = baseFreezeDuration;
        Amount             = baseAmount;
        Lifetime           = baseLifetime;
        Health             = baseHealth;
        Armor              = baseArmor;
        HealingMultiplier  = baseHealingMultiplier;
    }

    public void ApplyBonus(WeaponUpgradeDataSO bonus)
    {
        WeaponLevel  += bonus.WeaponLevel;
        UpgradeLevel += bonus.UpgradeLevel;
        Damage       += bonus.Damage;
        Speed        += bonus.Speed;
        Interval     -= bonus.Interval;   // reducing interval = faster fire
        SlowAmount   += bonus.SlowAmount;
        FreezeDuration += bonus.FreezeDuration;
        Amount       += bonus.Amount;
        Lifetime     += bonus.Lifetime;
        Health       += bonus.Health;
        Armor        += bonus.Armor;
        HealingMultiplier += bonus.HealingMultiplier;
    }

    public int GetUpgradeCost()
    {
        return UpgradeLevel <= 0
            ? baseWeaponUpgradeCost
            : baseWeaponUpgradeCost + (baseUpgradeCostPerLevel * UpgradeLevel);
    }
}
