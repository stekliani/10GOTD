using System;
using UnityEngine;

public class PlayerLevels : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    [Header("Level Settings")]
    [SerializeField] private float startingXpToNextLevel = 100f;
    [SerializeField] private float xpGrowthMultiplier    = 1.25f;

    [Header("Level-Up Reward")]
    [SerializeField] private StatsModifier levelUpBonus;

    public static event Action OnLevelUpRequested;

    private LevelSystem _levelSystem;

    private void Awake()
    {
        _levelSystem = new LevelSystem(startingXpToNextLevel, xpGrowthMultiplier);
        _levelSystem.OnLevelUpReady += HandleLevelUpReady;
    }

    private void OnEnable()  => UIManager.OnUpgradeChosen += HandleUpgradeChosen;
    private void OnDisable() => UIManager.OnUpgradeChosen -= HandleUpgradeChosen;

    public void AddXp(float rawAmount)
    {
        float finalAmount = rawAmount * (1f + playerStats.XpBonus / 100f);
        _levelSystem.AddXp(finalAmount);
    }

    public float GetXpToNextLevel() => _levelSystem.XpToNextLevel;
    public int   GetLevel()         => _levelSystem.Level;

    private void HandleLevelUpReady()
    {
        playerStats.ApplyRuntimeModifier(levelUpBonus);
        playerStats.Heal(playerStats.MaxHealth);
        SoundEventBus.Raise(SoundActions.playOnLevelUp);
        OnLevelUpRequested?.Invoke();
    }

    private void HandleUpgradeChosen()
    {
        _levelSystem.ConsumeUpgradeChosen();
    }

    public float GetCurrentXp() => _levelSystem.CurrentXp;
}
