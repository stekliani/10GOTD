using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private readonly List<Locale> _locales = new();
    private bool _suppressNotify;

    private void Reset()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private async void OnEnable()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();

        if (dropdown == null)
        {
            Debug.LogWarning($"{nameof(LanguageDropdown)} requires a TMP_Dropdown reference.", this);
            return;
        }

        var init = LocalizationSettings.InitializationOperation;
        if (!init.IsDone)
            await init.Task;

        PopulateOptions();

        dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;

        SyncDropdownToSelectedLocale();
    }

    private void OnDisable()
    {
        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);

        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    private void PopulateOptions()
    {
        _locales.Clear();
        dropdown.ClearOptions();

        var available = LocalizationSettings.AvailableLocales;
        if (available == null || available.Locales == null || available.Locales.Count == 0)
        {
            dropdown.AddOptions(new List<string> { "No locales" });
            dropdown.interactable = false;
            return;
        }

        var labels = new List<string>(available.Locales.Count);
        foreach (var locale in available.Locales)
        {
            if (locale == null)
                continue;

            _locales.Add(locale);

            // Prefer a human-readable name; fall back to code.
            string label = !string.IsNullOrEmpty(locale.LocaleName)
                ? locale.LocaleName
                : locale.Identifier.Code;

            labels.Add(label);
        }

        dropdown.AddOptions(labels);
        dropdown.interactable = _locales.Count > 1;
    }

    private void SyncDropdownToSelectedLocale()
    {
        var selected = LocalizationSettings.SelectedLocale;
        if (selected == null)
            return;

        int idx = _locales.FindIndex(l => l != null && l.Identifier == selected.Identifier);
        if (idx < 0)
            return;

        _suppressNotify = true;
        dropdown.SetValueWithoutNotify(idx);
        dropdown.RefreshShownValue();
        _suppressNotify = false;
    }

    private void OnDropdownValueChanged(int index)
    {
        if (_suppressNotify)
            return;

        if (index < 0 || index >= _locales.Count)
            return;

        Locale locale = _locales[index];
        if (locale == null)
            return;

        LocalizationSettings.SelectedLocale = locale;
        LocalizationPreferences.SaveSelectedLocale(locale);
    }

    private void OnSelectedLocaleChanged(Locale _)
    {
        SyncDropdownToSelectedLocale();
    }
}

