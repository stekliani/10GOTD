using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    private PlayerStats _player;
    [SerializeField] private GameObject upgradeWindow;
    [SerializeField] private GameObject settingsWindow;

    private string mainMenuSceneName = SceneLoader.Scene.Menu.ToString();
    private string level1SceneName = SceneLoader.Scene.Level1.ToString();
    private void Start() 
    {
        _player = GetComponent<PlayerStats>(); 
    }

    public void SceneChange(string sceneName)
    {
        _player?.ClearAllStatusEffects();
        _player?.ClearAllBuffs();
        _player?.ResetRuntimeModifiers();
        SceneLoader.Load(sceneName);
    }

    public void StartGame()
    {
        SceneLoader.Load(level1SceneName);
    }
    public void ToggleUpgradesWindow()
    {
        if (upgradeWindow.activeSelf)
        {
            upgradeWindow?.SetActive(false);
        }
        else
        {
            upgradeWindow?.SetActive(true);
            BaseStatsUpgradeManager.Instance?.PopulateUpgradesMenu();
        }
    }

    /// <summary>Wire menu buttons to this (scene-local) so UnityEvents survive menu reload; forwards to the DDOL singleton.</summary>
    public void SaveMenuMetaProgress() => BaseStatsUpgradeManager.Instance?.SaveUpgrades();

    public void CloseMenuMetaUpgradesWindow() => BaseStatsUpgradeManager.Instance?.CloseUpgradesWindow();

    public void ToggleSettingsWindow()
    {
        if(settingsWindow.activeSelf)
        {
            settingsWindow?.SetActive(false);
        }
        else
        {
            settingsWindow?.SetActive(true);
        }
    }

    public void ToMainMenu()
    {
        MainPoolManager.Instance.ReturnAllActiveObjects();
        SceneLoader.Load(mainMenuSceneName);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
