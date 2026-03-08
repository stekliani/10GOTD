using NUnit.Framework;

[TestFixture]
public class LevelSystemTests
{
    private LevelSystem _sut;

    [SetUp]
    public void SetUp() => _sut = new LevelSystem(startingXpThreshold: 100f, growthMultiplier: 1.25f);

    [Test]
    public void StartsAtLevelOne()
    {
        Assert.AreEqual(1, _sut.Level);
    }

    [Test]
    public void AddXp_BelowThreshold_DoesNotLevelUp()
    {
        _sut.AddXp(50f);
        Assert.AreEqual(1, _sut.Level);
    }

    [Test]
    public void AddXp_AtThreshold_IncrementsLevel()
    {
        bool eventFired = false;
        _sut.OnLevelUpReady += () => eventFired = true;
        _sut.AddXp(100f);
        Assert.AreEqual(2, _sut.Level);
        Assert.IsTrue(eventFired);
    }

    [Test]
    public void AddXp_OverThreshold_LeavesRemainder()
    {
        _sut.AddXp(150f);
        Assert.AreEqual(2, _sut.Level);
        Assert.AreEqual(50f, _sut.CurrentXp, delta: 0.01f);
    }

    [Test]
    public void XpToNextLevel_GrowsAfterLevelUp()
    {
        float initial = _sut.XpToNextLevel;
        _sut.AddXp(initial);
        Assert.Greater(_sut.XpToNextLevel, initial);
    }

    [Test]
    public void MultipleThresholds_LevelsUpMultipleTimes()
    {
        int levelUps = 0;
        _sut.OnLevelUpReady += () => levelUps++;

        _sut.AddXp(100f);
        float next = _sut.XpToNextLevel;
        _sut.AddXp(next + 50f);

        Assert.AreEqual(3, _sut.Level);
        Assert.AreEqual(2, levelUps);
    }
}
