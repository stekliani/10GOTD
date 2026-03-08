using System;
using UnityEngine;

[Serializable]
public class PlayerStatEntry
{
    public string        statNameKey;
    public Sprite        statSprite;
    public float         statValue;
    public int           maxUpgradeLevel;
    public int           currentUpgradeLevel = 0;
    public int           upgradeCostPerLevel;
    public StatsModifier runtimeUpgradeModifier;
    private const int BaseUpgradeCost = 20;

    [Header("Base Stat Values")]
    [Tooltip("For Upgrades In Menu!")]
    [SerializeField] private float baseStatUpgradeValue = 10;

    public int GetUpgradeCost()
    {
        int level = currentUpgradeLevel;
        return level <= 0
            ? BaseUpgradeCost
            : BaseUpgradeCost + (upgradeCostPerLevel * level);
    }

    public bool canUpgrade(int coinAmount, int level, int maxUpgradeLevel)
    {
        bool hasUpgradeLimit = maxUpgradeLevel > 0;
        bool isBelowUpgradeLimit;
        if (hasUpgradeLimit)
        {
            isBelowUpgradeLimit = level < maxUpgradeLevel;
        }
        else
        {
            isBelowUpgradeLimit = true;
        }


        if (coinAmount >= GetUpgradeCost() && isBelowUpgradeLimit)
            return true;
        else return false;
    }

    public float GetStatValue()
    {
        return statValue;
    }

    public float GetBaseStatValue()
    {
        return baseStatUpgradeValue;
    }

    public float UpgradeStatFromMenu()
    {
        currentUpgradeLevel++;

        statValue = baseStatUpgradeValue * currentUpgradeLevel;

        Debug.Log($"lvl: {currentUpgradeLevel}");

        return statValue;
    }


    public void ResetMenuUpgradeStats()
    {
        statValue = 0;
        currentUpgradeLevel = 0;
    }

}
