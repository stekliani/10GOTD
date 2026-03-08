using NUnit.Framework;

[TestFixture]
public class BuffSystemTests
{
    private BuffSystem _sut;

    [SetUp]
    public void SetUp() => _sut = new BuffSystem();

    [Test]
    public void ApplyBuff_AddsSingleModifier()
    {
        StatsModifier mod = new StatsModifier { maxHealth = 10f };
        _sut.ApplyBuff("hp_buff", mod, 5f, BuffStackType.RefreshDuration);
        Assert.AreEqual(10f, _sut.CachedTotal.maxHealth);
    }

    [Test]
    public void ApplyBuff_RefreshDuration_DoesNotDuplicate()
    {
        StatsModifier mod = new StatsModifier { armor = 5f };
        _sut.ApplyBuff("armor_buff", mod, 5f, BuffStackType.RefreshDuration);
        _sut.ApplyBuff("armor_buff", mod, 5f, BuffStackType.RefreshDuration);
        Assert.AreEqual(5f, _sut.CachedTotal.armor);
    }

    [Test]
    public void ApplyBuff_StackIntensity_DoublesModifier()
    {
        StatsModifier mod = new StatsModifier { damageBoost = 10f };
        _sut.ApplyBuff("dmg_buff", mod, 5f, BuffStackType.StackIntensity);
        _sut.ApplyBuff("dmg_buff", mod, 5f, BuffStackType.StackIntensity);
        Assert.AreEqual(20f, _sut.CachedTotal.damageBoost);
    }

    [Test]
    public void ApplyBuff_Independent_StacksSameId()
    {
        StatsModifier mod = new StatsModifier { recovery = 2f };
        _sut.ApplyBuff("heal_1", mod, 5f, BuffStackType.Independent);
        _sut.ApplyBuff("heal_1", mod, 5f, BuffStackType.Independent);
        Assert.AreEqual(4f, _sut.CachedTotal.recovery);
    }

    [Test]
    public void Tick_ExpiresBuffAfterDuration()
    {
        StatsModifier mod = new StatsModifier { piercing = 3f };
        _sut.ApplyBuff("pierce_buff", mod, 1f, BuffStackType.RefreshDuration);
        _sut.Tick(1.1f);
        Assert.AreEqual(0f, _sut.CachedTotal.piercing);
    }

    [Test]
    public void Tick_DoesNotExpireBeforeDuration()
    {
        StatsModifier mod = new StatsModifier { maxHealth = 50f };
        _sut.ApplyBuff("hp", mod, 5f, BuffStackType.RefreshDuration);
        _sut.Tick(3f);
        Assert.AreEqual(50f, _sut.CachedTotal.maxHealth);
    }

    [Test]
    public void ClearAll_RemovesAllBuffs()
    {
        _sut.ApplyBuff("a", new StatsModifier { armor = 10f }, 5f, BuffStackType.Independent);
        _sut.ApplyBuff("b", new StatsModifier { mana  = 20f }, 5f, BuffStackType.Independent);
        _sut.ClearAll();
        Assert.AreEqual(0f, _sut.CachedTotal.armor);
        Assert.AreEqual(0f, _sut.CachedTotal.mana);
    }
}
