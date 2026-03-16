using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float _timer;
    private SpawnManager _spawnManager;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _spawnManager = FindObjectOfType<SpawnManager>();
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

        int diamondsRewardAmount = 0;
        diamondsRewardAmount = ((int)_timer / 60) + GetDiamondsRewardFromWaves();
        BaseStatsUpgradeManager.Instance.AddDiamonds(diamondsRewardAmount);

        SaveManager.SaveAll();
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