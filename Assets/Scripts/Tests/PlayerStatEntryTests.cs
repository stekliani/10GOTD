using NUnit.Framework;

[TestFixture]
public class PlayerStatEntryTests
{
    private PlayerStatEntry _stat;

    [SetUp]
    public void SetUp()
    {
        _stat = new PlayerStatEntry
        {
            statNameKey            = "Armor",
            statValue           = 10f,
            upgradeCostPerLevel = 10,
            currentUpgradeLevel = 0
        };
    }

    [Test]
    public void GetUpgradeCost_AtLevelZero_ReturnsBaseCost()
    {
        Assert.AreEqual(20, _stat.GetUpgradeCost());
    }

    [Test]
    public void GetUpgradeCost_AtLevelOne_AddsPerLevelCost()
    {
        Assert.AreEqual(30, _stat.GetUpgradeCost());
    }

    [Test]
    public void GetUpgradeCost_ScalesWithLevel()
    {
        Assert.AreEqual(70, _stat.GetUpgradeCost());
    }

    [Test]
    public void GetUpgradeCost_NegativeLevel_TreatsAsZero()
    {
        Assert.AreEqual(20, _stat.GetUpgradeCost());
    }
}
