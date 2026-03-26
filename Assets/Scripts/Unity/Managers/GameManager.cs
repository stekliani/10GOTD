using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    private int _diamondsRewardFromMonsters;
    private float _timer;
    private bool _gameOverHandled;
    private SpawnManager _spawnManager;
    private PlayerStats _playerStats;
    private UIManager _uiManager;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _gameOverHandled = false;
        _spawnManager = FindObjectOfType<SpawnManager>();
        _playerStats = FindObjectOfType<PlayerStats>();
        _uiManager = FindObjectOfType<UIManager>();

        _diamondsRewardFromMonsters = 0;
    }

    private void OnEnable()
    {
        EventsManager.Instance.OnLastEnemyDeath += HandleLastEnemyDeath;
    }

    private void OnDisable()
    {
        EventsManager.Instance.OnLastEnemyDeath -= HandleLastEnemyDeath;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
    }

    private void HandleLastEnemyDeath(object sender, EventArgs e)
    {
        HandleGameOver();
    }



    private int totalDiamondsRewardAmount = 0;

    public void HandleGameOver()
    {
        // Guard against double-crediting diamonds when multiple damage sources
        // (multiple enemies) call Die() in the same moment.
        if (_gameOverHandled) return;
        _gameOverHandled = true;

        Time.timeScale = 0f;
        totalDiamondsRewardAmount = GetDiamondsRewardFromWaves() + _diamondsRewardFromMonsters;
        BaseStatsUpgradeManager.Instance.AddDiamonds(totalDiamondsRewardAmount);

        SaveManager.SaveAll();
        _playerStats.ResetRuntimeModifiers();

        _uiManager.OpenGameOverScreen();
        
        Debug.Log("Added" + totalDiamondsRewardAmount + "Diamonds");
    }


    //Diamonds Reward
    int diamondsRewardFromWaves;
    private void CalculateDiamondsRewardFromWaves()
    {
        diamondsRewardFromWaves = 0;
        diamondsRewardFromWaves += _spawnManager.GetCompletedWavesDiamondReward();
    }

    private int GetDiamondsRewardFromWaves()
    {
        CalculateDiamondsRewardFromWaves();
        return diamondsRewardFromWaves;
    }

    public void IncreaseDiamondsRewardFromMonsters(int diamondsRewardAmount)
    {
        _diamondsRewardFromMonsters += diamondsRewardAmount;
    }

    public int GetTotalDiamondsRewardAmount()
    {
        return totalDiamondsRewardAmount;
    }
    public float GetTimer() => _timer;
}