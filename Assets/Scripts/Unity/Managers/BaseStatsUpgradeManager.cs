using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
public class BaseStatsUpgradeManager :
    MonoBehaviour,
    ISaveable<BaseStatsUpgradeManager.BaseStatsUpgradeSaveData>
{
    [SerializeField] private GameObject contentGameobject;
    [SerializeField] private GameObject MainMenuUpgradeTemplate;
    [SerializeField] private PlayerDataSO playerDataSO;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private int diamonds;

    public string SaveKey => "BaseStatsUpgradeData";

    private void Start()
    {
        // Reset to defaults first (in case LoadAll doesn't find save data)
        ResetUpgrades();
        diamonds = 0;
        
        // Load save data, then populate menu with loaded values
        SaveManager.LoadAll();
        PopulateUpgradesMenu();

        Debug.Log(Application.persistentDataPath);
    }

    public void PopulateUpgradesMenu()
    {

        PlayerStatEntry[] stats = GetEntityStats();

        foreach (Transform child in contentGameobject.transform)
        {
            Destroy(child.gameObject);
        }


        foreach (PlayerStatEntry statEntry in stats)
        {
            var stat = Instantiate(MainMenuUpgradeTemplate, contentGameobject.transform);
            var cached = statEntry;

            var statImage = stat.GetComponentInChildren<Image>();
            var statButton = stat.GetComponentInChildren<Button>();
            var statButtonText = statButton.GetComponentInChildren<TextMeshProUGUI>();
            var localizer = stat.GetComponentInChildren<LocalizeStringEvent>();
            var buttonTextLocalizer = statButton.GetComponentInChildren<LocalizeStringEvent>();

            L.PlayerStatLocalizer(localizer, cached);

            statImage.sprite = cached.statSprite;
            statButton.interactable = cached.canUpgrade(diamonds, cached.currentUpgradeLevel, cached.maxUpgradeLevel);
            statButton.onClick.AddListener(() =>
            {

                cached.UpgradeStatFromMenu();
                diamonds -= cached.GetUpgradeCost();
                PopulateUpgradesMenu();
            });

            if (statButton.interactable || cached.currentUpgradeLevel < cached.maxUpgradeLevel)
            {
                L.ButtonLocalizer(buttonTextLocalizer, "UI.upgradeCost");

                buttonTextLocalizer.StringReference.TableReference = "In Game UI";
                buttonTextLocalizer.StringReference.TableEntryReference = "UI.upgradeCost";
                buttonTextLocalizer.StringReference.Arguments =
                    new object[] { cached.GetUpgradeCost() };

                buttonTextLocalizer.RefreshString();
            }
            else
            {
                L.ButtonLocalizer(buttonTextLocalizer, "UI.maxLevel");

                statButtonText.text =
                    buttonTextLocalizer.StringReference.GetLocalizedString();
            }
        }
    }

    public void SaveUpgradeS()
    {
        SaveManager.SaveAll();
        upgradePanel.gameObject.SetActive(false);
    }


    private void ResetUpgrades()
    {
        PlayerStatEntry[] stats = GetEntityStats();
        foreach (PlayerStatEntry entity in stats)
        {
            entity.ResetMenuUpgradeStats();
        }
    }


    private PlayerStatEntry[] GetEntityStats()
    {
        PlayerStatEntry[] stats =
{
        playerDataSO.maxHealth,
        playerDataSO.mana,
        playerDataSO.manaRegen,
        playerDataSO.damageBoost,
        playerDataSO.xpBonus,
        playerDataSO.recovery,
        playerDataSO.cooldownReduction,
        playerDataSO.armor,
        playerDataSO.piercing,
        playerDataSO.amount,
        playerDataSO.projectileSpeed,
        playerDataSO.area
    };
        return stats;
    }

    public BaseStatsUpgradeSaveData CaptureState()
    {
        var stats = GetEntityStats();
        var statArr = new BaseStatsUpgradeSaveData.StatData[stats.Length];

        for (int i = 0; i < stats.Length; i++)
        {
            statArr[i] = new BaseStatsUpgradeSaveData.StatData
            {
                statKey = stats[i].statNameKey,
                level = stats[i].currentUpgradeLevel
            };
        }

        return new BaseStatsUpgradeSaveData
        {
            diamonds = diamonds,
            stats = statArr
        };
    }

    object ISaveable.CaptureState() => CaptureState();

    public void RestoreState(BaseStatsUpgradeSaveData data)
    {
        // Handle null or missing save data - use default values
        if (data == null)
        {
            Debug.Log("No save data found for BaseStatsUpgradeManager. Using default values.");
            ResetUpgrades();
            diamonds = 0;
            return;
        }

        diamonds = data.diamonds;

        var stats = GetEntityStats();

        // Handle null or empty stats array
        if (data.stats == null || data.stats.Length == 0)
        {
            Debug.Log("Save data has no stats. Resetting upgrades.");
            ResetUpgrades();
            return;
        }

        foreach (var saved in data.stats)
        {
            // Skip null entries
            if (saved == null) continue;

            var entry = Array.Find(stats, s => s.statNameKey == saved.statKey);
            if (entry == null) continue;

            entry.ResetMenuUpgradeStats();

            // Apply upgrades up to saved level
            for (int i = 0; i < saved.level; i++)
                entry.UpgradeStatFromMenu();
        }
    }

    void ISaveable.RestoreState(object state)
    {
        // Handle null state
        if (state == null)
        {
            RestoreState(null);
            return;
        }

        try
        {
            RestoreState((BaseStatsUpgradeSaveData)state);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to restore BaseStatsUpgradeManager state: {e.Message}. Using default values.");
            ResetUpgrades();
            diamonds = 0;
        }
    }

    [Serializable]
    public class BaseStatsUpgradeSaveData
    {
        [Serializable]
        public class StatData
        {
            public string statKey;
            public int level;
        }

        public int diamonds;
        public StatData[] stats;
    }
}
