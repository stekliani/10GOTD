using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class BaseStatsUpgradeManager :
    MonoBehaviour,
    ISaveable<BaseStatsUpgradeManager.BaseStatsUpgradeSaveData>
{
    [SerializeField] private GameObject contentGameobject;
    [SerializeField] private GameObject MainMenuUpgradeTemplate;
    [SerializeField] private PlayerDataSO playerDataSO;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private TextMeshProUGUI diamondsText;
    [SerializeField] private TextMeshProUGUI applyButtonText;
    [SerializeField] private TextMeshProUGUI closeButtonText;
    [SerializeField] private int diamonds;

    private LocalizeStringEvent diamondsLocalizeEvent;
    private UnityAction<string> setDiamondsTextAction;
    private LocalizeStringEvent applyButtonLocalizeEvent;
    private UnityAction<string> setApplyButtonTextAction;
    private LocalizeStringEvent closeButtonLocalizeEvent;
    private UnityAction<string> setCloseButtonTextAction;
    public string SaveKey => "BaseStatsUpgradeData";

    public static BaseStatsUpgradeManager Instance {  get; private set; }

    private void OnEnable()
    {
        if (contentGameobject != null && MainMenuUpgradeTemplate != null)
            PopulateUpgradesMenu();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.AdoptMenuUiFrom(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleMenuSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleMenuSceneLoaded;
    }

    /// <summary>
    /// When the menu scene loads again, a second BaseStatsUpgradeManager exists with valid UI refs.
    /// The singleton (DDOL) keeps save state; we copy fresh scene references onto it before destroying the duplicate.
    /// </summary>
    private void HandleMenuSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance == null) return;
        if (!scene.name.Equals(SceneLoader.Scene.Menu.ToString(), StringComparison.Ordinal))
            return;

        foreach (var m in FindObjectsOfType<BaseStatsUpgradeManager>(true))
        {
            if (m == Instance) continue;
            Instance.AdoptMenuUiFrom(m);
            Destroy(m.gameObject);
            break;
        }
    }

    /// <summary>
    /// Copies UI references from a freshly loaded menu instance onto the persistent singleton.
    /// </summary>
    public void AdoptMenuUiFrom(BaseStatsUpgradeManager other)
    {
        if (other == null) return;

        contentGameobject = other.contentGameobject;
        MainMenuUpgradeTemplate = other.MainMenuUpgradeTemplate;
        playerDataSO = other.playerDataSO;
        upgradePanel = other.upgradePanel;
        diamondsText = other.diamondsText;
        applyButtonText = other.applyButtonText;
        closeButtonText = other.closeButtonText;

        diamondsLocalizeEvent = null;
        applyButtonLocalizeEvent = null;
        closeButtonLocalizeEvent = null;
        setDiamondsTextAction = null;
        setApplyButtonTextAction = null;
        setCloseButtonTextAction = null;

        PopulateUpgradesMenu();
    }

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
        if (contentGameobject == null || MainMenuUpgradeTemplate == null)
            return;

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
                // Cost must be read before UpgradeStatFromMenu — GetUpgradeCost() uses currentUpgradeLevel,
                // which increments inside UpgradeStatFromMenu, so reading after would charge the *next* tier.
                int cost = cached.GetUpgradeCost();
                cached.UpgradeStatFromMenu();
                diamonds -= cost;
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

        RefreshDiamondsLocalizedText();
        RefreshApplyButtonLocalizedText();
        RefreshCloseButtonLocalizedText();
    }

    public void SaveUpgrades()
    {
        SaveManager.SaveAll();
    }


    public void CloseUpgradesWindow()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
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

    public void AddDiamonds(int value)
    {
        diamonds += value;
    }

    public int GetDiamondsAmount()
    {
        return diamonds;
    }

    private void RefreshDiamondsLocalizedText()
    {
        if (diamondsText == null)
            return;

        diamondsLocalizeEvent ??= diamondsText.GetComponent<LocalizeStringEvent>();
        if (diamondsLocalizeEvent == null)
            return;

        // In the scene, the LocalizeStringEvent may not have its UpdateString UnityEvent wired.
        // Wire it at runtime so RefreshString actually pushes into the TMP text.
        setDiamondsTextAction ??= (value) => diamondsText.text = value;
        diamondsLocalizeEvent.OnUpdateString.RemoveListener(setDiamondsTextAction);
        diamondsLocalizeEvent.OnUpdateString.AddListener(setDiamondsTextAction);

        L.MainMenuDiamondsLocalizer(diamondsLocalizeEvent, this);
    }

    private void RefreshApplyButtonLocalizedText()
    {
        if (applyButtonText == null) return;

        applyButtonLocalizeEvent ??= applyButtonText.GetComponent<LocalizeStringEvent>();
        if(applyButtonLocalizeEvent == null) return;


        // In the scene, the LocalizeStringEvent may not have its UpdateString UnityEvent wired.
        // Wire it at runtime so RefreshString actually pushes into the TMP text.
        setApplyButtonTextAction ??= (value) => applyButtonText.text = value;
        applyButtonLocalizeEvent.OnUpdateString.RemoveListener(setApplyButtonTextAction);
        applyButtonLocalizeEvent.OnUpdateString.AddListener(setApplyButtonTextAction);

        L.MainMenuUpgradesButtonsLocalizer(applyButtonLocalizeEvent, "Menu.Upgrades.apply");
    }
    private void RefreshCloseButtonLocalizedText()
    {
        if (closeButtonText == null) return;

        closeButtonLocalizeEvent ??= closeButtonText.GetComponent<LocalizeStringEvent>();
        if (closeButtonLocalizeEvent == null) return;


        // In the scene, the LocalizeStringEvent may not have its UpdateString UnityEvent wired.
        // Wire it at runtime so RefreshString actually pushes into the TMP text.
        setCloseButtonTextAction ??= (value) => closeButtonText.text = value;
        closeButtonLocalizeEvent.OnUpdateString.RemoveListener(setCloseButtonTextAction);
        closeButtonLocalizeEvent.OnUpdateString.AddListener(setCloseButtonTextAction);

        L.MainMenuUpgradesButtonsLocalizer(closeButtonLocalizeEvent, "Menu.Upgrades.close");
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
