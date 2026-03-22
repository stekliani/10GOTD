using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float _timer;
    private SpawnManager _spawnManager;
    private PlayerStats _playerStats;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Time.timeScale = 1f;

        _spawnManager = FindObjectOfType<SpawnManager>();
        _playerStats = FindObjectOfType<PlayerStats>();
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

    public void HandleGameOver()
    {
        Time.timeScale = 0f;
        int diamondsRewardAmount = 0;
        diamondsRewardAmount = ((int)_timer / 60) + GetDiamondsRewardFromWaves();
        BaseStatsUpgradeManager.Instance.AddDiamonds(diamondsRewardAmount);

        SaveManager.SaveAll();
        _playerStats.ResetRuntimeModifiers();
        Debug.Log("Added" + diamondsRewardAmount + "Diamonds");
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

    public float GetTimer() => _timer;
}