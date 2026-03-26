using System;
using UnityEngine;

public abstract class Weapon : Subject, IWeapon
{
    [SerializeField] protected WeaponDataSO                 baseData;
    [SerializeField] protected WeaponOnLevelUpUpgradeDataSO levelUpUpgradeData;
    [SerializeField] private   WeaponUpgradeDataSO          runtimeUpgradeData;

    protected WeaponDataSO data;
    protected IPlayerStats player;
    protected float        timer;

    private const int MaxLevel = 3;

    protected float Area => player.Area;
    public bool IsActive { get; set; } = false;
    public int  GetLevel()    => data.WeaponLevel;
    public int  GetMaxLevel() => MaxLevel;

    public static event Action<Weapon> OnWeaponSpawned;

    protected void Awake()
    {
        data = Instantiate(baseData);
        data.InitializeWeaponDefaults();
        OnWeaponSpawned?.Invoke(this);
    }

    protected virtual void Start()
    {
        if (player != null)
            timer = GetModifiedInterval();
    }

    public virtual void Initialize(IPlayerStats playerStats)
    {
        player = playerStats;
        timer  = GetModifiedInterval();
    }

    protected virtual void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f && CanFire())
        {
            Fire();
            timer = GetModifiedInterval();
        }
    }

    protected virtual float GetModifiedInterval()
    {
        if (player == null) return data != null ? data.Interval : 1f;
        float multiplier = 1f + player.CooldownReduction / 100f;
        return data.Interval / multiplier;
    }

    protected virtual bool CanFire() => IsActive;

    protected abstract void Fire();

    public void UpgradeWeapon(WeaponOnLevelUpUpgradeDataSO upgrade)
    {
        data.WeaponLevel += upgrade.WeaponLevel;
        data.Damage      += upgrade.Damage;
        data.Speed       += upgrade.Speed;
        data.Interval    -= upgrade.Interval;
        data.Area        += upgrade.Area;
        data.Lifetime    += upgrade.Lifetime;
        data.SlowAmount += upgrade.SlowAmount;
        data.FreezeDuration += upgrade.FreezeDuration;
        data.Amount += upgrade.Amount;
        data.Health      += upgrade.Health;
        data.Armor       += upgrade.Armor;
        data.HealingMultiplier += upgrade.HealingMultiplier;
    }

    public virtual void ApplyWeaponRuntimeLevelUpUpgrade()
    {
        if (runtimeUpgradeData == null) return;
        if (data == null) return;
        // Runtime upgrades increase `WeaponDataSO.UpgradeLevel` (used for cost),
        // so cap based on UpgradeLevel (not WeaponLevel).
        if (data.UpgradeLevel >= MaxLevel) return;

        data.ApplyBonus(runtimeUpgradeData);

        NotifyObservers(InputActions.UpgradeRuntimeStats);
    }

    public WeaponDataSO                 GetWeaponData()                  => data;
    public WeaponOnLevelUpUpgradeDataSO GetWeaponOnLevelUpUpgradeData()  => levelUpUpgradeData;
}
