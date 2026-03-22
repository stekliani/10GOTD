using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour, IEnemyTarget
{
    private Collider2D[] _colliders;
    private SpriteRenderer[] _spriteRenderers;
    public float currentHealth;
    public float respawnTime;
    public bool isCountingDown = false;
    private WallWeapon _owner;

    private void Awake()
    {
        _owner = FindObjectOfType<WallWeapon>();
    }
    private void OnEnable()
    {
        if (_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider2D>(includeInactive: true);
        foreach (Collider2D col in _colliders)
            if (col != null) col.enabled = true;

        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (SpriteRenderer r in _spriteRenderers)
            if (r != null) r.enabled = true;
    }

    private void OnDisable()
    {
        if (_colliders != null)
            foreach (Collider2D col in _colliders)
                if (col != null) col.enabled = false;

        if (_spriteRenderers != null)
            foreach (SpriteRenderer r in _spriteRenderers)
                if (r != null) r.enabled = false;
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
        SetCollidersEnabled(false);
        SetSpriteRenderersEnabled(false);

        yield return new WaitForSeconds(respawnTime);

        // Restore health back to the wall's max health when respawning.
        _owner.InitializeWallHealth(this);
        isCountingDown = false;
        SetCollidersEnabled(true);
        SetSpriteRenderersEnabled(true);
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider2D>(includeInactive: true);

        foreach (Collider2D col in _colliders)
            if (col != null) col.enabled = enabled;
    }

    private void SetSpriteRenderersEnabled(bool enabled)
    {
        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        foreach (SpriteRenderer r in _spriteRenderers)
            if (r != null) r.enabled = enabled;
    }
}