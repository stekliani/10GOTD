using NUnit.Framework;

[TestFixture]
public class StatusEffectSystemTests
{
    private class StubDamageable : IDamageable
    {
        public float DamageTaken { get; private set; }
        public void TakeDamage(float damage) => DamageTaken += damage;
    }

    [Test]
    public void BurnEffect_DealsDamageOverTime()
    {
        var system = new StatusEffectSystem();
        var target = new StubDamageable();
        system.Apply(new BurnEffect(dps: 10f, duration: 2f), target);
        system.Tick(target, 1f);
        Assert.AreEqual(10f, target.DamageTaken, delta: 0.01f);
    }

    [Test]
    public void PoisonEffect_DealsDamageOncePerSecond()
    {
        var system = new StatusEffectSystem();
        var target = new StubDamageable();
        system.Apply(new PoisonEffect(dps: 5f, duration: 3f), target);
        system.Tick(target, 0.5f);
        Assert.AreEqual(0f, target.DamageTaken, "Poison has not ticked at 0.5s.");
        system.Tick(target, 0.6f);
        Assert.AreEqual(5f, target.DamageTaken, delta: 0.01f);
    }

    [Test]
    public void StatusEffect_ExpiresAfterDuration()
    {
        var system = new StatusEffectSystem();
        var target = new StubDamageable();
        system.Apply(new BurnEffect(dps: 10f, duration: 1f), target);
        system.Tick(target, 1.5f);
        float dmgAfterExpiry = target.DamageTaken;
        system.Tick(target, 1f);
        Assert.AreEqual(dmgAfterExpiry, target.DamageTaken);
    }

    [Test]
    public void ClearAll_StopsAllEffects()
    {
        var system = new StatusEffectSystem();
        var target = new StubDamageable();
        system.Apply(new BurnEffect(10f, 5f), target);
        system.ClearAll();
        system.Tick(target, 1f);
        Assert.AreEqual(0f, target.DamageTaken);
    }
}
