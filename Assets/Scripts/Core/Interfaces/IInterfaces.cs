using UnityEngine;
using UnityEngine.Localization.Components;

// ── INPUT OBSERVER ───────────────────────────────────────────────────────────
public interface IInputObserver
{
    void OnNotify(InputActions action);
}

// ── ANIMATION OBSERVER ───────────────────────────────────────────────────────
public interface IAnimationObserver
{
    void OnNotify(AnimationActions action, Vector3 position);
}

// ── SOUNDS OBSERVER ─────────────────────────────────────────────────────────
public interface ISoundsObserver
{
    void OnNotify(SoundActions action);
}
// ── DAMAGEABLE ───────────────────────────────────────────────────────────────
public interface IDamageable
{
    void TakeDamage(float damage);
}

public interface IEnemyTarget
{
    void TakeDamage(float damage);
}

// ── HEALABLE ─────────────────────────────────────────────────────────────────
public interface IHealable
{
    void Heal(float value);
}

// ── SLOWABLE ─────────────────────────────────────────────────────────────────
public interface ISlowable
{
    void ApplySlow(float amount);
    void RemoveSlow(float amount);
}

// ── WEAPON ───────────────────────────────────────────────────────────────────
public interface IWeapon
{
    bool IsActive { get; set; }
    int  GetLevel();
    int  GetMaxLevel();
    void Initialize(IPlayerStats player);
}

// ── PLAYER STATS — read-only view ────────────────────────────────────────────
public interface IPlayerStats
{
    float CurrentHealth     { get; }
    float MaxHealth         { get; }
    float DamageBoost       { get; }
    float ProjectileSpeed   { get; }
    float CooldownReduction { get; }
    float Armor             { get; }
    float Piercing          { get; }
    int Amount            { get; }
    float Xp                { get; }
    float XpBonus           { get; }
    float Recovery          { get; }
    float Area { get; }
}

// ── PLAYER MUTATOR — write operations ────────────────────────────────────────
public interface IPlayerMutator : IPlayerStats
{
    void TakeDamage(float damage);
    void Heal(float value);
    void AddXp(float amount);
    void ConsumeXp(float amount);
    void ApplyRuntimeModifier(StatsModifier modifier);
    void ResetRuntimeModifiers();
}



public interface ISaveable
{
    string SaveKey { get; }
    object CaptureState();
    void RestoreState(object state);
}

public interface ISaveable<T> : ISaveable
{
    new T CaptureState();
    void RestoreState(T state);
}