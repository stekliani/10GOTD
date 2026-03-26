using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class UIManager : MonoBehaviour, IInputObserver
{
    public static event Action OnUpgradeChosen;

    [SerializeField] private InputManager inputManager;
    [SerializeField] private UpgradeManager upgradeManager;

    [Header("Stats Window")]
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject template;

    [Header("Bars")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image xpBarImage;

    [Header("Runtime Upgrades")]
    [SerializeField] private GameObject contentGameobject;
    [SerializeField] private GameObject runtimeUpgradeTemplate;

    [Header("Level-Up Menu")]
    [SerializeField] private GameObject levelUpScreenPanel;
    [SerializeField] private GameObject actualLevelUpScreen;
    [SerializeField] private GameObject weaponUpgradeTemplate;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI waveIndexText;
    [SerializeField] private GameObject pauseWindow;

    private const int MaxUpgradeOptions = 4;

    private PlayerStats _playerStats;
    private PlayerInventory _playerInventory;
    private PlayerLevels _playerLevels;
    private SpawnManager _spawnManager;
    private string _coinsLabel;
    private string _xpLabel;

    private bool statsInitialized = false;

    private Dictionary<PlayerStatEntry, PlayerStatUI> statUIMap = new();
    private Dictionary<Weapon, WeaponUI> weaponUIMap = new();

    private class PlayerStatUI
    {
        public Image image;
        public Button button;
        public TextMeshProUGUI buttonText;
        public LocalizeStringEvent statLocalizer;
        public LocalizeStringEvent buttonLocalizer;
    }

    class WeaponUI
    {
        public GameObject root;
        public Image image;
        public Button button;

        public LocalizeStringEvent weaponLocalizer;
        public LocalizeStringEvent buttonLocalizer;
    }

    private enum weaponType
    {
        other,
        healingFountain,
        snowballWeapon,
        fireball,
        wall,
        slowingWeapon,
    }

    weaponType type;

    private void Awake()
    {
        if (inputManager == null) inputManager = FindObjectOfType<InputManager>();
        if (upgradeManager == null) upgradeManager = FindObjectOfType<UpgradeManager>();

        _playerStats = FindObjectOfType<PlayerStats>();
        _playerInventory = FindObjectOfType<PlayerInventory>();
        _playerLevels = FindObjectOfType<PlayerLevels>();
        _spawnManager = FindObjectOfType<SpawnManager>();
    }

    private void Start()
    {
        UpdateRuntimeUpgradesWindow();
    }

    private void Update()
    {
        UpdateHealthBar();
        UpdateXPBar();

        if (coinsText != null && _playerInventory != null)
            coinsText.text = _coinsLabel + ": " + _playerInventory.GetCoinAmount();

        if (xpText != null && _playerLevels != null)
            xpText.text = _xpLabel + ": " + _playerLevels.GetCurrentXp() + "/" + _playerLevels.GetXpToNextLevel();
        if (hpText != null && _playerStats != null)
            hpText.text = _playerStats.CurrentHealth + "/" + _playerStats.MaxHealth;
        if(waveIndexText != null && _spawnManager != null)
        {
            UpdateWaveIndexText();
        }
    }

    private void OnEnable()
    {
        inputManager?.AddObserver(this);
        upgradeManager?.AddObserver(this);
        _playerInventory?.AddObserver(this);

        Weapon.OnWeaponSpawned += RegisterWeapon;
        PlayerLevels.OnLevelUpRequested += OpenLevelUpScreen;
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

        RefreshHudLabels();
    }

    private void OnDisable()
    {
        inputManager?.RemoveObserver(this);
        upgradeManager?.RemoveObserver(this);
        _playerInventory?.RemoveObserver(this);

        Weapon.OnWeaponSpawned -= RegisterWeapon;
        PlayerLevels.OnLevelUpRequested -= OpenLevelUpScreen;
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    #region Update Displays
    private void OnSelectedLocaleChanged(UnityEngine.Localization.Locale _)
    {
        RefreshHudLabels();
        UpdatePlayerStatsUI();
    }

    private void RefreshHudLabels()
    {
        _coinsLabel = L.GetPlayerInventoryStat("Inventory.coins");
        _xpLabel = L.GetPlayerStat("Stat.xp");
    }

    public void OnNotify(InputActions action)
    {
        switch (action)
        {
            case InputActions.OpenStatsWindow:
                OpenStatsWindow();
                break;
            case InputActions.OpenLevelUpWindow:
                OpenOrCloseLevelUpWindow();
                break;
            case InputActions.UpgradeRuntimeStats:
                UpdateRuntimeUpgradesWindow();
                break;
        }
    }

    private void InitializePlayerStatsUI()
    {
        foreach (PlayerStatEntry stat in _playerStats.GetStats())
        {
            GameObject go = Instantiate(runtimeUpgradeTemplate, contentGameobject.transform);

            var ui = new PlayerStatUI();

            ui.image = go.GetComponentInChildren<Image>();
            ui.button = go.GetComponentInChildren<Button>();
            ui.buttonText = ui.button.GetComponentInChildren<TextMeshProUGUI>();

            var localizers = go.GetComponentsInChildren<LocalizeStringEvent>();

            if (localizers.Length >= 2)
            {
                ui.statLocalizer = localizers[0];
                ui.buttonLocalizer = localizers[1];
            }

            ui.image.sprite = stat.statSprite;

            ui.button.onClick.AddListener(() =>
            {
                upgradeManager.UpgradePlayerStat(stat, ui.button);
                UpdatePlayerStatsUI();
            });

            statUIMap[stat] = ui;
        }

        statsInitialized = true;
    }

    private void UpdatePlayerStatsUI()
    {
        foreach (var pair in statUIMap)
        {
            var stat = pair.Key;
            var ui = pair.Value;

            L.PlayerStatLocalizer(ui.statLocalizer, _playerStats, stat);

            bool isInteractable = stat.canUpgrade(
                _playerInventory.GetCoinAmount(),
                stat.currentUpgradeLevel,
                stat.maxUpgradeLevel
            );

            bool hasMaxLevel = stat.maxUpgradeLevel > 0;
            bool isMaxed = hasMaxLevel && stat.currentUpgradeLevel >= stat.maxUpgradeLevel;

            if (stat.statNameKey == "Stat.armor")
                isMaxed = _playerStats.Armor >= 90;

            ui.button.interactable = !isMaxed && isInteractable;

            if (!isMaxed)
            {
                L.ButtonLocalizer(ui.buttonLocalizer, "UI.upgradeCost");

                ui.buttonLocalizer.StringReference.TableReference = "In Game UI";
                ui.buttonLocalizer.StringReference.TableEntryReference = "UI.upgradeCost";
                ui.buttonLocalizer.StringReference.Arguments =
                    new object[] { stat.GetUpgradeCost() };

                ui.buttonLocalizer.RefreshString();
            }
            else
            {
                L.ButtonLocalizer(ui.buttonLocalizer, "UI.maxLevel");

                ui.buttonText.text =
                    ui.buttonLocalizer.StringReference.GetLocalizedString();
            }
        }
    }

    private void UpdateRuntimeUpgradesWindow()
    {
        // ALWAYS initialize stats first
        if (!statsInitialized)
            InitializePlayerStatsUI();

        // Always update stats first
        UpdatePlayerStatsUI();

        // THEN weapons
        UpdateWeaponsDisplay();
    }

    private void UpdateHealthBar()
    {
        healthBarImage.fillAmount =
            Mathf.Clamp01(_playerStats.CurrentHealth / _playerStats.MaxHealth);
    }

    private void UpdateXPBar()
    {
        xpBarImage.fillAmount = Mathf.Clamp01(_playerLevels.GetCurrentXp() / _playerLevels.GetXpToNextLevel());
    }

    private void UpdateWeaponsDisplay()
    {
        List<Weapon> activeWeapons = upgradeManager.GetActivatedWeaponsList();

        foreach (Weapon weapon in activeWeapons)
        {
            // Ensure UI exists
            CreateWeaponUI(weapon);

            var ui = weaponUIMap[weapon];
            var data = weapon.GetWeaponData();

            ui.image.sprite = data.weaponSprite;

            string weaponEntryKey;

            if (weapon is HealingFountain)
                type = weaponType.healingFountain;
            else if (weapon is SnowballWeapon)
                type = weaponType.snowballWeapon;
            else if (weapon is WallWeapon)
                type = weaponType.wall;
            else if (weapon is SlowingWeapon)
                type = weaponType.slowingWeapon;
            else
                type = weaponType.other;

            switch (type)
            {
                default:
                    weaponEntryKey = "Weapon.damageInfo";
                    break;
                case weaponType.healingFountain:
                    weaponEntryKey = "Weapon.healingInfo";
                    break;
                case weaponType.snowballWeapon:
                    weaponEntryKey = "Weapon.freezeInfo";
                    break;
                case weaponType.wall:
                    weaponEntryKey = "Weapon.wallInfo";
                    break;
                case weaponType.slowingWeapon:
                    weaponEntryKey = "Weapon.slowInfo";
                    break;
            }

            string localizedWeaponName = L.GetLocalizedWeaponName(data.weaponName);

            L.WeaponLocalizer(ui.weaponLocalizer, weapon, _playerStats, weaponEntryKey, localizedWeaponName);

            ui.button.interactable =
                _playerInventory.GetCoinAmount() >= data.GetUpgradeCost() &&
                data.UpgradeLevel < weapon.GetMaxLevel();

            L.WeaponButtonLocalizer(ui.buttonLocalizer, localizedWeaponName, data);
        }
    }

    public void UpdateWaveIndexText()
    {
        waveIndexText.text = _spawnManager.GetCurrentWaveIndex().ToString();
    }
    #endregion

    #region Open/Close windows
    private void OpenStatsWindow()
    {
        if (!background.activeSelf)
        {
            background.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            background.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void OpenLevelUpScreen()
    {
        levelUpScreenPanel.SetActive(true);
        PopulateLevelUpWindow();
        Time.timeScale = 0f;
    }

    private void OpenOrCloseLevelUpWindow()
    {
        if (levelUpScreenPanel.activeSelf)
            levelUpScreenPanel.SetActive(false);
        else
        {
            levelUpScreenPanel.SetActive(true);
            PopulateLevelUpWindow();
        }
    }

    private void UpgradeWeaponAndCloseWindow(Weapon weapon)
    {
        bool firstTime = !weapon.IsActive;

        weapon.IsActive = true;

        if(weapon is HealingFountain  && !weapon.gameObject.activeSelf)
        {
            weapon.gameObject.SetActive(true);
        }
        if (!firstTime)
            weapon.UpgradeWeapon(weapon.GetWeaponOnLevelUpUpgradeData());

        levelUpScreenPanel.SetActive(false);
        OnUpgradeChosen?.Invoke();
        Time.timeScale = 1f;

        UpdateRuntimeUpgradesWindow();
    }
    #endregion
    #region Create/Populate
    private void PopulateLevelUpWindow()
    {
        foreach (Transform child in actualLevelUpScreen.transform)
        {
            if (child.GetComponent<Decoration>()) continue;
            Destroy(child.gameObject);
        }

        List<Weapon> options = upgradeManager.GetRandomUpgrades(MaxUpgradeOptions);

        foreach (Weapon option in options)
        {
            GameObject go = Instantiate(weaponUpgradeTemplate, actualLevelUpScreen.transform);
            Weapon captured = option;

            var helper = go.GetComponent<TemplateHelper>();
            var data = option.GetWeaponData();

            string localizedWeaponName = L.GetLocalizedWeaponName(data.weaponName);
            string localizedLevel = L.Get("In Game UI", "UI.level");

            helper.image.sprite = data.weaponSprite;
            helper.text.text = localizedWeaponName + "\n" + localizedLevel + ": " + data.WeaponLevel;

            helper.button.onClick.AddListener(() =>
                UpgradeWeaponAndCloseWindow(captured));
        }
    }

    private void CreateWeaponUI(Weapon weapon)
    {
        if (weaponUIMap.ContainsKey(weapon))
            return;

        GameObject go = Instantiate(runtimeUpgradeTemplate, contentGameobject.transform);

        var ui = new WeaponUI();
        ui.root = go;

        ui.image = go.GetComponentInChildren<Image>();
        ui.button = go.GetComponentInChildren<Button>();

        var localizers = go.GetComponentsInChildren<LocalizeStringEvent>();

        if (localizers.Length >= 2)
        {
            ui.weaponLocalizer = localizers[0];
            ui.buttonLocalizer = localizers[1];
        }

        ui.button.onClick.AddListener(() =>
        {
            // Runtime weapon upgrades should cost coins and stop at max upgrade level.
            int cost = weapon.GetWeaponData().GetUpgradeCost();
            if (_playerInventory.GetCoinAmount() < cost) return;
            if (weapon.GetWeaponData().UpgradeLevel >= weapon.GetMaxLevel()) return;

            _playerInventory.RemoveCoins(cost);
            weapon.ApplyWeaponRuntimeLevelUpUpgrade();
        });

        weaponUIMap[weapon] = ui;
    }
    #endregion
    private void RegisterWeapon(Weapon weapon)
    {
        weapon.AddObserver(this);
        if (statsInitialized)
        {
            UpdateWeaponsDisplay();
        }
    }
}