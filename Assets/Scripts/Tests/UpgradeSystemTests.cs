using System;
using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class UpgradeSystemTests
{
    private class StubWeapon : IWeapon
    {
        private readonly int _level;
        private readonly int _maxLevel;

        public StubWeapon(int level, int maxLevel) { _level = level; _maxLevel = maxLevel; }

        public bool IsActive { get; set; }
        public int  GetLevel()    => _level;
        public int  GetMaxLevel() => _maxLevel;
        public void Initialize(IPlayerStats player) { }
    }

    [Test]
    public void FilterEligible_IncludesWeaponsUnderMaxLevel()
    {
        var weapons = new List<IWeapon>
        {
            new StubWeapon(level: 0, maxLevel: 3),
            new StubWeapon(level: 2, maxLevel: 3),
        };
        Assert.AreEqual(2, UpgradeSystem.FilterEligible(weapons).Count);
    }

    [Test]
    public void FilterEligible_ExcludesWeaponsAtMaxLevel()
    {
        var weapons = new List<IWeapon> { new StubWeapon(level: 4, maxLevel: 3) };
        Assert.AreEqual(0, UpgradeSystem.FilterEligible(weapons).Count);
    }

    [Test]
    public void FilterEligible_IgnoresNullEntries()
    {
        var weapons = new List<IWeapon> { null, new StubWeapon(0, 3) };
        Assert.AreEqual(1, UpgradeSystem.FilterEligible(weapons).Count);
    }

    [Test]
    public void GetRandomUpgrades_ReturnsRequestedAmount()
    {
        var pool = new List<IWeapon>
        {
            new StubWeapon(0,3), new StubWeapon(1,3),
            new StubWeapon(2,3), new StubWeapon(0,3),
        };
        Assert.AreEqual(3, UpgradeSystem.GetRandomUpgrades(pool, 3, new Random(42)).Count);
    }

    [Test]
    public void GetRandomUpgrades_NoDuplicates()
    {
        var pool = new List<IWeapon>
        {
            new StubWeapon(0,3), new StubWeapon(1,3), new StubWeapon(2,3),
        };
        var result = UpgradeSystem.GetRandomUpgrades(pool, 3, new Random(99));
        Assert.AreEqual(result.Count, new HashSet<IWeapon>(result).Count);
    }

    [Test]
    public void GetRandomUpgrades_CapsAtPoolSize()
    {
        var pool = new List<IWeapon> { new StubWeapon(0, 3) };
        Assert.AreEqual(1, UpgradeSystem.GetRandomUpgrades(pool, 10, new Random()).Count);
    }

    [Test]
    public void GetRandomUpgrades_EmptyPool_ReturnsEmpty()
    {
        Assert.AreEqual(0, UpgradeSystem.GetRandomUpgrades(new List<IWeapon>(), 4, new Random()).Count);
    }
}
