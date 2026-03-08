using System;

[Serializable]
public struct StatsModifier
{
    public float maxHealth;
    public float mana;
    public float manaRegen;
    public float damageBoost;
    public float xpBonus;
    public float recovery;
    public float cooldownReduction;
    public float armor;
    public float piercing;
    public int amount;
    public float projectileSpeed;
    public float cameraSize;
    public float area;

    public static StatsModifier Zero => new StatsModifier();

    public static StatsModifier operator +(StatsModifier a, StatsModifier b)
    {
        return new StatsModifier
        {
            maxHealth         = a.maxHealth         + b.maxHealth,
            mana              = a.mana              + b.mana,
            manaRegen         = a.manaRegen         + b.manaRegen,
            damageBoost       = a.damageBoost       + b.damageBoost,
            xpBonus           = a.xpBonus           + b.xpBonus,
            recovery          = a.recovery          + b.recovery,
            cooldownReduction = a.cooldownReduction - b.cooldownReduction,
            armor             = a.armor             + b.armor,
            piercing          = a.piercing          + b.piercing,
            amount            = a.amount            + b.amount,
            projectileSpeed   = a.projectileSpeed   + b.projectileSpeed,
            cameraSize        = a.cameraSize        + b.cameraSize,
            area              = a.area              + b.area
        };
    }
}
