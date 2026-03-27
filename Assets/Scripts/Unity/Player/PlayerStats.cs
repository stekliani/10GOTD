using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : Subject, IPlayerMutator, IHealable, IEnemyTarget
{
    public static event EventHandler OnHeal;

    [SerializeField] private PlayerDataSO data;

    [Header("Base Stats")]
    [SerializeField] private PlayerStatEntry maxHealth;
    [SerializeField] private PlayerStatEntry mana;
    [SerializeField] private PlayerStatEntry manaRegen;
    [SerializeField] private PlayerStatEntry damageBoost;
    [SerializeField] private PlayerStatEntry xpBonus;
    [SerializeField] private PlayerStatEntry recovery;
    [SerializeField] private PlayerStatEntry cooldownReduction;
    [SerializeField] private PlayerStatEntry armor;
    [SerializeField] private PlayerStatEntry piercing;
    [SerializeField] private PlayerStatEntry amount;
    [SerializeField] private PlayerStatEntry projectileSpeed;
    [SerializeField] private PlayerStatEntry area;

    private float _currentHealth;
    private float _currentMana;
    private float _xp;
    private StatsModifier _runtimeModifier;
    private bool _isDead;

    private readonly BuffSystem         _buffSystem         = new();
    private readonly StatusEffectSystem _statusEffectSystem = new();

    private PlayerStatEntry[] _allStats;
    private HealingFountain   _healingFountain;

    private void Awake()
    {
        _allStats = new PlayerStatEntry[]
        {
            maxHealth, mana,manaRegen, damageBoost, xpBonus,
            recovery, cooldownReduction, armor,
            piercing, amount, projectileSpeed,area
        };



        _currentHealth = MaxHealth;
        _currentMana = mana.statValue;
    }

    private void Start()
    {
        _healingFountain = FindObjectOfType<HealingFountain>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        _buffSystem.Tick(dt);

        //-------------------------------------------------------------------------
        //_statusEffectSystem.Tick(this, dt); implement if need to debuff player
        //-------------------------------------------------------------------------

        float multiplier = 1f;
        if (_healingFountain == null)
        {
            _healingFountain = FindObjectOfType<HealingFountain>();
        }
        if (_healingFountain != null && _healingFountain.IsActive)
        {
            multiplier = _healingFountain.GetHealingAmountMultiplier();
        }
        if (_currentHealth < MaxHealth)
        {
            Heal(Recovery * dt * multiplier);
            Debug.Log($"recovery:{Recovery} multiplier: {multiplier}");
        }

        if (_currentMana < MaxMana)
        {
            RecoverMana(ManaRegen * dt);
        }
    }

    // IPlayerStats (read)
    private StatsModifier Total => _runtimeModifier + _buffSystem.CachedTotal;

    public float CurrentHealth     => _currentHealth;
    public float CurrentMana       => _currentMana;
    public float MaxHealth         => maxHealth.statValue           + Total.maxHealth           + data.maxHealth.statValue;
    public float MaxMana           => mana.statValue                + Total.mana                + data.mana.statValue;
    public float ManaRegen         => manaRegen.statValue           + Total.manaRegen           + data.manaRegen.statValue;
    public float DamageBoost       => damageBoost.statValue         + Total.damageBoost         + data.damageBoost.statValue;
    public float XpBonus           => xpBonus.statValue             + Total.xpBonus             + data.xpBonus.statValue;
    public float Recovery          => recovery.statValue            + Total.recovery            + data.recovery.statValue;
    public float CooldownReduction => cooldownReduction.statValue   + Total.cooldownReduction   + data.cooldownReduction.statValue;
    public float Armor             => armor.statValue               + Total.armor               + data.armor.statValue;
    public float Piercing          => piercing.statValue            + Total.piercing            + data.piercing.statValue;
    public int Amount              => (int)amount.statValue         + (int)Total.amount         + (int)data.amount.statValue;
    public float ProjectileSpeed   => projectileSpeed.statValue     + Total.projectileSpeed     + data.projectileSpeed.statValue;
    public float Area              => area.statValue                + Total.area                + data.area.statValue;
    public float Xp                => _xp;

    public string SaveKey => throw new NotImplementedException();

    // IPlayerMutator (write)
    public void TakeDamage(float damage)
    {
        float reduction = Mathf.Clamp(Armor / 100f, 0f, 0.9f);
        float finalDmg  = damage * (1f - reduction);
        _currentHealth  = Mathf.Clamp(_currentHealth - finalDmg, 0, MaxHealth);
        if (_currentHealth <= 0 && !_isDead) Die();
    }

    public void Heal(float value)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + value, 0, MaxHealth);
        OnHeal?.Invoke(this, EventArgs.Empty);
    }

    public void RecoverMana(float value)
    {
        _currentMana = Mathf.Clamp(_currentMana + value, 0, MaxMana);
    }

    public void AddXp(float amount)    => _xp += amount;

    public void ConsumeXp(float amount)
    {
        _xp -= amount;
        if (_xp < 0) _xp = 0;
    }

    public void ApplyRuntimeModifier(StatsModifier mod)
    {
        _runtimeModifier += mod;
        _currentHealth    = Mathf.Clamp(_currentHealth, 0, MaxHealth);
    }

    public void ResetRuntimeModifiers()
    {
        _runtimeModifier = StatsModifier.Zero;
        _currentHealth   = Mathf.Clamp(_currentHealth, 0, MaxHealth);
    }

    //  Buff API
    public void ApplyBuff(BuffDefinitionSO buff)
    {
        _buffSystem.ApplyBuff(buff.buffID, buff.modifier, buff.duration, buff.stackType);
        _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
    }

    public void ClearAllBuffs()
    {
        _buffSystem.ClearAll();
        _currentHealth = Mathf.Clamp(_currentHealth, 0, MaxHealth);
    }

    // Status Effect API 
    public void ApplyStatusEffect(StatusEffectDefinitionSO def)
    {
        StatusEffect effect = def.type switch
        {
            StatusEffectDefinitionSO.StatusEffectType.Poison => new PoisonEffect(def.dps, def.duration),
            StatusEffectDefinitionSO.StatusEffectType.Burn   => new BurnEffect(def.dps, def.duration),
            _                                                 => null
        };

        //------------------------------------------------------------------------------------------------
        //if (effect != null) _statusEffectSystem.Apply(effect, this); implement if need to debuff player
        //------------------------------------------------------------------------------------------------
    }

    public void ClearAllStatusEffects() => _statusEffectSystem.ClearAll();

    // Stats display helpers
    public PlayerStatEntry[] GetStats() => _allStats;

    public float GetFinalStatValue(PlayerStatEntry stat)
    {
        if (stat == maxHealth)         return MaxHealth;
        if (stat == mana)              return MaxMana;
        if (stat == manaRegen)         return ManaRegen;
        if (stat == damageBoost)       return DamageBoost;
        if (stat == xpBonus)           return XpBonus;
        if (stat == recovery)          return Recovery;
        if (stat == cooldownReduction) return CooldownReduction;
        if (stat == armor)             return Armor;
        if (stat == piercing)          return Piercing;
        if (stat == amount)            return Amount;
        if (stat == projectileSpeed)   return ProjectileSpeed;
        if (stat == area)              return Area;
        return stat.statValue;
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        GameManager.Instance.HandleGameOver();
        Debug.Log("You Lose!");
    }
}
