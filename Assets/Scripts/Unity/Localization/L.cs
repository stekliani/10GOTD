using UnityEngine.Android;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

public static class L
{
    public static string GetPlayerStat(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("Player Stats", key);
    }
    public static string GetPlayerInventoryStat(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("Player Inventory", key);
    }


    public static string Get(string table, string key, params object[] args)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(table, key, args);
    }

    public static void PlayerStatLocalizer(LocalizeStringEvent localizer, PlayerStatEntry cached)
    {
        localizer.StringReference.TableReference = "Player Stats";
        localizer.StringReference.TableEntryReference = cached.statNameKey;

        localizer.StringReference.Arguments = new object[]
        {
            cached.statValue
        };

        localizer.RefreshString();
    }

    /// <summary>
    /// Localizes a player stat using the *final* in‑game value
    /// (base + runtime modifiers + data), so UI reflects upgrades.
    /// </summary>
    public static void PlayerStatLocalizer(
        LocalizeStringEvent localizer,
        PlayerStats         stats,
        PlayerStatEntry     cached)
    {
        localizer.StringReference.TableReference      = "Player Stats";
        localizer.StringReference.TableEntryReference = cached.statNameKey;

        float finalValue = stats.GetFinalStatValue(cached);

        localizer.StringReference.Arguments = new object[]
        {
            finalValue
        };

        localizer.RefreshString();
    }
    public static void ButtonLocalizer(LocalizeStringEvent localizer, string key)
    {
        localizer.StringReference.TableReference = "In Game UI";
        localizer.StringReference.TableEntryReference = key;
        localizer.RefreshString();
    }


    public static void WeaponLocalizer(
    LocalizeStringEvent localizer,
    Weapon weapon,
    string entryKey,
    string weaponName)
    {
        var data = weapon.GetWeaponData();

        localizer.StringReference.TableReference = "Weapon Stats";
        localizer.StringReference.TableEntryReference = entryKey;

        localizer.StringReference.Arguments = new object[]
        {
        weaponName,
        data.WeaponLevel,
        data.Damage,
        data.HealingMultiplier
        };

        localizer.RefreshString();
    }

    public static void WeaponButtonLocalizer(LocalizeStringEvent buttonLocalizer, string weaponName, WeaponDataSO data)
    {
        buttonLocalizer.StringReference.TableReference = "In Game UI";
        buttonLocalizer.StringReference.TableEntryReference = "UI.upgradeWeapon";
        buttonLocalizer.StringReference.Arguments =
            new object[] { weaponName, data.GetUpgradeCost() };

        buttonLocalizer.RefreshString();
    }

    public static void WeaponUpgradeCardLocalizer(
    LocalizeStringEvent localizer,
    Weapon weapon,
    string localizedWeaponName)
    {
        var data = weapon.GetWeaponData();
        localizer.StringReference.TableReference = "In Game UI";
        localizer.StringReference.TableEntryReference = "UI.weaponUpgradeCard";
        localizer.StringReference.Arguments = new object[]
        {
        localizedWeaponName,
        data.WeaponLevel
        };
        localizer.RefreshString();
    }

    public static string GetLocalizedWeaponName(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString("Weapon Name", key);
    }
}