using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SettingsTextsLocalizer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI backgroundMusicText;
    [SerializeField] private TextMeshProUGUI sfxMusicText;
    [SerializeField] private TextMeshProUGUI uiMusicText;
    [SerializeField] private TextMeshProUGUI languageText;
    [SerializeField] private TextMeshProUGUI closeButtonText;


    private string _menuUITableName = "Menu UI";
    private void OnEnable()
    {
        backgroundMusicText.text = L.Get(_menuUITableName, "Menu.Settings.backgroundMusic");
        sfxMusicText.text = L.Get(_menuUITableName, "Menu.Settings.sfxMusic");
        uiMusicText.text = L.Get(_menuUITableName, "Menu.Settings.uiMusic");
        languageText.text = L.Get(_menuUITableName, "Menu.Settings.language");
        closeButtonText.text = L.Get(_menuUITableName, "Menu.Upgrades.close");
    }
}
