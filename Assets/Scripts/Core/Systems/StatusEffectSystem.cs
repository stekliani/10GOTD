using System.Collections.Generic;

public abstract class StatusEffect
{
    public float RemainingTime;

    public virtual void OnApply(IDamageable target)           { }
    public virtual void OnTick(IDamageable target, float dt)  { }
    public virtual void OnExpire(IDamageable target)          { }
}

public class PoisonEffect : StatusEffect
{
    private readonly float _dps;
    private float _tickTimer;

    public PoisonEffect(float dps, float duration)
    {
        _dps          = dps;
        RemainingTime = duration;
    }

    public override void OnTick(IDamageable target, float dt)
    {
        _tickTimer += dt;
        if (_tickTimer >= 1f)
        {
            target.TakeDamage(_dps);
            _tickTimer = 0f;
        }
    }
}

public class BurnEffect : StatusEffect
{
    private readonly float _dps;

    public BurnEffect(float dps, float duration)
    {
        _dps          = dps;
        RemainingTime = duration;
    }

    public override void OnTick(IDamageable target, float dt)
    {
        target.TakeDamage(_dps * dt);
    }
}

public class StatusEffectSystem
{
    private readonly List<StatusEffect> _effects = new();

    public void Apply(StatusEffect effect, IDamageable target)
    {
        effect.OnApply(target);
        _effects.Add(effect);
    }

    public void Tick(IDamageable target, float dt)
    {
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            StatusEffect e = _effects[i];
            e.RemainingTime -= dt;
            e.OnTick(target, dt);

            if (e.RemainingTime <= 0)
            {
                e.OnExpire(target);
                _effects.RemoveAt(i);
            }
        }
    }

    public void ClearAll() => _effects.Clear();
}
