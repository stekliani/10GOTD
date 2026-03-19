using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IEnemyTarget
{
    private Collider2D[] _colliders;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float respawnTime;
    [HideInInspector] public bool isCountingDown = false;

    private void OnEnable()
    {
        _colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in _colliders)
            col.enabled = true;
    }

    private void OnDisable()
    {
        foreach (Collider2D col in _colliders)
            col.enabled = false;
    }

    public void InitializeHealth(float amount)
    {
        currentHealth = amount;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }

    // Called by WallWeapon when health hits 0
    public void StartRespawn()
    {
        if (!isCountingDown)
            StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isCountingDown = true;
        gameObject.SetActive(false);

        yield return new WaitForSeconds(respawnTime);

        InitializeHealth(currentHealth); // reset health tracked from outside
        isCountingDown = false;
        gameObject.SetActive(true);
    }
}