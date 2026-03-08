using System;

public class LevelSystem
{
    private float _xpToNextLevel;
    private readonly float _xpGrowthMultiplier;

    public int   Level           { get; private set; } = 1;
    public int   PendingLevelUps { get; private set; } = 0;
    public float CurrentXp       { get; private set; } = 0f;
    public float XpToNextLevel   => _xpToNextLevel;

    public event Action OnLevelUpReady;

    public LevelSystem(float startingXpThreshold, float growthMultiplier)
    {
        _xpToNextLevel      = startingXpThreshold;
        _xpGrowthMultiplier = growthMultiplier;
    }

    public void AddXp(float amount)
    {
        CurrentXp += amount;
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        while (CurrentXp >= _xpToNextLevel)
        {
            CurrentXp      -= _xpToNextLevel;
            _xpToNextLevel += (_xpToNextLevel + 20f) * _xpGrowthMultiplier;
            PendingLevelUps++;
        }

        if (PendingLevelUps > 0)
            ProcessNext();
    }

    private void ProcessNext()
    {
        if (PendingLevelUps <= 0) return;
        Level++;
        PendingLevelUps--;
        OnLevelUpReady?.Invoke();
    }

    public void ConsumeUpgradeChosen()
    {
        if (PendingLevelUps > 0)
            ProcessNext();
    }
}
