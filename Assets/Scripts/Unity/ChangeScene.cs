using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    private PlayerStats _player;
    [SerializeField] private GameObject upgradeWindow;
    [SerializeField] private BaseStatsUpgradeManager baseStatsUpgradeManager;

    private string mainMenuSceneName = "Menu";
    private string level1SceneName = "Level-1";
    private void Start() 
    {
        _player = GetComponent<PlayerStats>(); 
    }

    public void SceneChange(string sceneName)
    {
        _player?.ClearAllStatusEffects();
        _player?.ClearAllBuffs();
        _player?.ResetRuntimeModifiers();
        SceneManager.LoadScene(sceneName);
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
            baseStatsUpgradeManager.PopulateUpgradesMenu();
        }
    }

    public void ToMainMenu()
    {
        SceneChange(mainMenuSceneName);
    }

    public void ToLevel1()
    {
        SceneChange(level1SceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
