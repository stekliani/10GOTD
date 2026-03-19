using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class LocalizationPreferences
{
    private const string SelectedLocaleCodeKey = "SelectedLocaleCode";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedLocaleOnBoot()
    {
        // Fire-and-forget: Unity will keep the async op alive.
        _ = ApplySavedLocaleAsync();
    }

    public static void SaveSelectedLocale(Locale locale)
    {
        if (locale == null)
            return;

        string code = locale.Identifier.Code;
        if (string.IsNullOrEmpty(code))
            return;

        PlayerPrefs.SetString(SelectedLocaleCodeKey, code);
        PlayerPrefs.Save();
    }

    public static string LoadSavedLocaleCode()
    {
        return PlayerPrefs.GetString(SelectedLocaleCodeKey, string.Empty);
    }

    public static bool TryFindLocaleByCode(string code, out Locale locale)
    {
        locale = null;
        if (string.IsNullOrEmpty(code))
            return false;

        var available = LocalizationSettings.AvailableLocales;
        if (available == null)
            return false;

        // Match against the Identifier.Code (e.g. "en", "en-US", "ru-RU", "ka").
        foreach (var l in available.Locales)
        {
            if (l == null)
                continue;

            if (string.Equals(l.Identifier.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                locale = l;
                return true;
            }
        }

        return false;
    }

    private static async System.Threading.Tasks.Task ApplySavedLocaleAsync()
    {
        AsyncOperationHandle init = LocalizationSettings.InitializationOperation;
        if (!init.IsDone)
            await init.Task;

        string code = LoadSavedLocaleCode();
        if (string.IsNullOrEmpty(code))
            return;

        if (TryFindLocaleByCode(code, out var locale) && locale != null)
            LocalizationSettings.SelectedLocale = locale;
    }
}

