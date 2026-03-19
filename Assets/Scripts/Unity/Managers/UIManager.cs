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

    [SerializeField] private InputManager   inputManager;
    [SerializeField] private UpgradeManager upgradeManager;

    [Header("Stats Window")]
    [SerializeField] private GameObject Background;
    [SerializeField] private GameObject Template;

    [Header("Health Bar")]
    [SerializeField] private Image HealthBarImage;

    [Header("Runtime Upgrades")]
    [SerializeField] private GameObject contentGameobject;
    [SerializeField] private GameObject runtimeUpgradeTemplate;

    [Header("Level-Up Menu")]
    [SerializeField] private GameObject LevelUpScreenPanel;
    [SerializeField] private GameObject actualLevelUpScreen;
    [SerializeField] private GameObject weaponUpgradeTemplate;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI xpText;

    private const int MaxUpgradeOptions = 4;

    private PlayerStats     _playerStats;
    private PlayerInventory _playerInventory;
    private PlayerLevels    _playerLevels;

    TemplateHelper templateHelper;

    private string _coinsLabel;
    private string _xpLabel;
    private void Awake()
    {
        if (inputManager   == null) inputManager   = FindObjectOfType<InputManager>();
        if (upgradeManager == null) upgradeManager = FindObjectOfType<UpgradeManager>();

        _playerStats     = FindObjectOfType<PlayerStats>();
        _playerInventory = FindObjectOfType<PlayerInventory>();
        _playerLevels    = FindObjectOfType<PlayerLevels>();
    }
    private void Start()
    {
        UpdateRuntimeUpgradesWindow();
    }

    private void Update()
    {
        UpdateHealthBar();
        if (coinsText != null && _playerInventory != null)
            coinsText.text = $"{_coinsLabel}: {_playerInventory.GetCoinAmount()}";

        if (xpText != null && _playerLevels != null)
            xpText.text = $"{_xpLabel}: {_playerLevels.GetCurrentXp()}/{_playerLevels.GetXpToNextLevel()}";
    }

    private void OnEnable()
    {
        if (inputManager   == null) inputManager   = FindObjectOfType<InputManager>();
        if (upgradeManager == null) upgradeManager = FindObjectOfType<UpgradeManager>();
        if (_playerInventory == null) _playerInventory = FindObjectOfType<PlayerInventory>();

        inputManager?.AddObserver(this);
        upgradeManager?.AddObserver(this);
        _playerInventory?.AddObserver(this);
        Weapon.OnWeaponSpawned          += RegisterWeapon;
        PlayerLevels.OnLevelUpRequested += OpenLevelUpScreen;
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

        RefreshHudLabels();
    }

    private void OnDisable()
    {
        inputManager?.RemoveObserver(this);
        upgradeManager?.RemoveObserver(this);
        _playerInventory?.RemoveObserver(this);
        Weapon.OnWeaponSpawned          -= RegisterWeapon;
        PlayerLevels.OnLevelUpRequested -= OpenLevelUpScreen;
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    private void OnSelectedLocaleChanged(UnityEngine.Localization.Locale _)
    {
        RefreshHudLabels();
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
            case InputActions.OpenStatsWindow:     OpenStatsWindow();          break;
            case InputActions.OpenLevelUpWindow:   OpenOrCloseLevelUpWindow(); break;
            case InputActions.UpgradeRuntimeStats: UpdateRuntimeUpgradesWindow(); break;
        }
    }
    #region Open/close Windows
    public void OpenStatsWindow()
    {
        if (!Background.activeSelf)
        {
            UpdateStatsDisplay();
            Background.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Background.SetActive(false);
            Time.timeScale = 1f;
        }
    }
    private void OpenLevelUpScreen()
    {
        LevelUpScreenPanel.SetActive(true);
        PopulateLevelUpWindow();
        Time.timeScale = 0f;
    }
    public void OpenOrCloseLevelUpWindow()
    {
        if (LevelUpScreenPanel.activeSelf)
            LevelUpScreenPanel.SetActive(false);
        else
        {
            LevelUpScreenPanel.SetActive(true);
            PopulateLevelUpWindow();
        }
    }
    #endregion

    #region Update/Populate Windows
    public void UpdateStatsDisplay()
    {
        foreach (Transform child in Background.transform)
            Destroy(child.gameObject);

        foreach (PlayerStatEntry stat in _playerStats.GetStats())
        {
            GameObject go = Instantiate(Template, Background.transform);

            var templateImage = go.GetComponentInChildren<Image>();
            var localizer = go.GetComponentInChildren<UnityEngine.Localization.Components.LocalizeStringEvent>();
            var cached = stat;

            go.SetActive(true);
            templateImage.sprite = stat.statSprite;
            // Use final in‑game value (includes runtime modifiers) for display
            L.PlayerStatLocalizer(localizer, _playerStats, cached);
        }
    }

    public void PopulateLevelUpWindow()
    {
        foreach (Transform child in actualLevelUpScreen.transform)
        {
            if (child.gameObject.TryGetComponent<Decoration>(out Decoration d)) continue;

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
            helper.text.text = $"{localizedWeaponName}\n{localizedLevel}: {data.WeaponLevel}";

            helper.button.onClick.AddListener(() => UpgradeWeaponAndCloseWindow(captured));
        }
    }

    public void UpgradeWeaponAndCloseWindow(Weapon weapon)
    {
        bool firstTime = !weapon.IsActive;

        weapon.IsActive = true;

        // only apply upgrade stats if weapon was already active before
        if (!firstTime)
            weapon.UpgradeWeapon(weapon.GetWeaponOnLevelUpUpgradeData());


        LevelUpScreenPanel.SetActive(false);
        OnUpgradeChosen?.Invoke();
        Time.timeScale = 1f;
        UpdateRuntimeUpgradesWindow();
    }

    private void UpdateHealthBar()
    {
        HealthBarImage.fillAmount =
            Mathf.Clamp01(_playerStats.CurrentHealth / _playerStats.MaxHealth);
    }

    private void UpdateRuntimeUpgradesWindow()
    {
        foreach (Transform child in contentGameobject.transform)
            Destroy(child.gameObject);


        //Player Stat Upgrades
        UpdatePlayerStatsDisplay();

        //Weapon Upgrades
        UpdateWeaponsDisplay();
    }
    #endregion


    private void UpdateWeaponsDisplay()
    {
        foreach (Weapon weapon in upgradeManager.GetActivatedWeaponsList())
        {
            GameObject runtimeUpgradeTemplate = Instantiate(this.runtimeUpgradeTemplate, contentGameobject.transform);
            Weapon captured = weapon;

            var data = weapon.GetWeaponData();

            // --- Components ---
            Image image = runtimeUpgradeTemplate.GetComponentInChildren<Image>();
            Button weaponUpgradeButton = runtimeUpgradeTemplate.GetComponentInChildren<Button>();

            LocalizeStringEvent weaponTextLocalizer =
                runtimeUpgradeTemplate.GetComponentInChildren<LocalizeStringEvent>();

            LocalizeStringEvent buttonLocalizer =
                weaponUpgradeButton.GetComponentInChildren<LocalizeStringEvent>();

            // --- Sprite ---
            image.sprite = data.weaponSprite;

            // --- Choose localization entry ---
            string weaponEntryKey =
                weapon is HealingFountain
                ? "Weapon.healingInfo"
                : "Weapon.damageInfo";

            // --- LOCALIZE WEAPON TEXT ---
            string localizedWeaponName = L.GetLocalizedWeaponName(data.weaponName);

            L.WeaponLocalizer(weaponTextLocalizer, weapon,_playerStats, weaponEntryKey, localizedWeaponName);

            // --- Button state ---
            weaponUpgradeButton.interactable =
                _playerInventory.GetCoinAmount() >= data.GetUpgradeCost();

            // --- LOCALIZE BUTTON TEXT ---
            L.WeaponButtonLocalizer(buttonLocalizer, localizedWeaponName, data);

            // --- Upgrade action ---
            weaponUpgradeButton.onClick.AddListener(() =>
                captured.ApplyWeaponRuntimeLevelUpUpgrade());
        }

    }
    private void UpdatePlayerStatsDisplay()
    {
        foreach (PlayerStatEntry statEntry in _playerStats.GetStats())
        {
            var stat = Instantiate(runtimeUpgradeTemplate, contentGameobject.transform);
            var cached = statEntry;

            var statImage = stat.GetComponentInChildren<Image>();
            var statButton = stat.GetComponentInChildren<Button>();
            var statButtonText = statButton.GetComponentInChildren<TextMeshProUGUI>();
            var localizer = stat.GetComponentInChildren<LocalizeStringEvent>();
            var buttonTextLocalizer = statButton.GetComponentInChildren<LocalizeStringEvent>();

            // Show the current final value (base + runtime upgrades + data)
            L.PlayerStatLocalizer(localizer, _playerStats, cached);

            statImage.sprite = cached.statSprite;
            statButton.interactable = cached.canUpgrade(_playerInventory.GetCoinAmount(), cached.currentUpgradeLevel, cached.maxUpgradeLevel);
            statButton.onClick.AddListener(() =>
            {
                upgradeManager.UpgradePlayerStat(cached, statButton);
            });

            bool hasMaxLevel = cached.maxUpgradeLevel > 0;
            bool isMaxed = hasMaxLevel && cached.currentUpgradeLevel >= cached.maxUpgradeLevel;

            if (!isMaxed)
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
    private void RegisterWeapon(Weapon weapon) => weapon.AddObserver(this);
}
