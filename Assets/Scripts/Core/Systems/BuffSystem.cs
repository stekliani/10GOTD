using System.Collections.Generic;

public enum BuffStackType
{
    RefreshDuration,
    StackIntensity,
    Independent
}

public class ActiveBuff
{
    public string        Id;
    public StatsModifier Modifier;
    public float         RemainingTime;
    public int           Stacks;
    public BuffStackType StackType;

    public ActiveBuff(string id, StatsModifier mod, float duration, BuffStackType type)
    {
        Id            = id;
        Modifier      = mod;
        RemainingTime = duration;
        StackType     = type;
        Stacks        = 1;
    }
}

public class BuffSystem
{
    private readonly List<ActiveBuff> _activeBuffs = new();

    public StatsModifier CachedTotal { get; private set; }

    public void ApplyBuff(string id, StatsModifier mod, float duration, BuffStackType type)
    {
        if (type == BuffStackType.Independent)
        {
            _activeBuffs.Add(new ActiveBuff(id, mod, duration, type));
            RecalculateStats();
            return;
        }

        ActiveBuff existing = _activeBuffs.Find(b => b.Id == id);

        if (existing == null)
        {
            _activeBuffs.Add(new ActiveBuff(id, mod, duration, type));
        }
        else if (type == BuffStackType.RefreshDuration)
        {
            existing.RemainingTime = duration;
        }
        else if (type == BuffStackType.StackIntensity)
        {
            existing.Stacks++;
            existing.RemainingTime = duration;
        }

        RecalculateStats();
    }

    public bool Tick(float dt)
    {
        bool changed = false;

        for (int i = _activeBuffs.Count - 1; i >= 0; i--)
        {
            _activeBuffs[i].RemainingTime -= dt;
            if (_activeBuffs[i].RemainingTime <= 0)
            {
                _activeBuffs.RemoveAt(i);
                changed = true;
            }
        }

        if (changed) RecalculateStats();
        return changed;
    }

    public void ClearAll()
    {
        _activeBuffs.Clear();
        CachedTotal = StatsModifier.Zero;
    }

    private void RecalculateStats()
    {
        StatsModifier total = StatsModifier.Zero;

        foreach (ActiveBuff buff in _activeBuffs)
        {
            total.maxHealth         += buff.Modifier.maxHealth         * buff.Stacks;
            total.mana              += buff.Modifier.mana              * buff.Stacks;
            total.damageBoost       += buff.Modifier.damageBoost       * buff.Stacks;
            total.recovery          += buff.Modifier.recovery          * buff.Stacks;
            total.cooldownReduction += buff.Modifier.cooldownReduction * buff.Stacks;
            total.armor             += buff.Modifier.armor             * buff.Stacks;
            total.piercing          += buff.Modifier.piercing          * buff.Stacks;
            total.amount            += buff.Modifier.amount            * buff.Stacks;
            total.projectileSpeed   += buff.Modifier.projectileSpeed   * buff.Stacks;
            total.area              += buff.Modifier.area              * buff.Stacks;
        }

        CachedTotal = total;
    }
}
