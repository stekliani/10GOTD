using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float _timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
        Debug.Log("Game Over1234");
    }

    public float GetTimer() => _timer;
}