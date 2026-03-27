using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Fireball : MonoBehaviour
{
    private Collider2D[] _colliders;
    private SpriteRenderer[] _spriteRenderers;
    private ParticleSystem[] _particleSystems;

    // Lifetime
    public bool isCountingDown = false;
    private float _activeTime;
    private float _respawnTime;
    private Coroutine _lifetimeCoroutine;

    // Combat
    private float _damage;
    private float _piercing;

    // Orbit state
    private float _orbitAngle;
    private float _orbitRadius;

    private void OnEnable()
    {
        CacheComponents();
        SetCollidersEnabled(true);
        SetSpritesEnabled(true);
        SetParticlesEnabled(true);
    }

    private void OnDisable()
    {
        SetCollidersEnabled(false);
        SetSpritesEnabled(false);
        SetParticlesEnabled(false);
    }

    // ---------- Orbit ----------
    public void SetOrbitParams(float angle, float radius)
    {
        _orbitAngle = angle;
        _orbitRadius = radius;
    }

    public void UpdatePosition(Transform playerTransform)
    {
        if (playerTransform == null) return;

        float rad = _orbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * _orbitRadius;
        transform.position = playerTransform.position + offset;
    }

    // ---------- Setup ----------
    public void Initialize(float activeTime, float respawnTime)
    {
        _activeTime = activeTime;
        _respawnTime = respawnTime;
    }

    public void SetDamage(float damage, float piercing)
    {
        _damage = damage;
        _piercing = piercing;
    }

    public void StartLifetime()
    {
        if (_lifetimeCoroutine != null)
            StopCoroutine(_lifetimeCoroutine);

        _lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
    }

    public void CancelAndRestore()
    {
        if (_lifetimeCoroutine != null)
        {
            StopCoroutine(_lifetimeCoroutine);
            _lifetimeCoroutine = null;
        }

        isCountingDown = false;
        SetCollidersEnabled(true);
        SetSpritesEnabled(true);
        SetParticlesEnabled(true);
    }

    // ---------- Core Loop (Snowball-style) ----------
    private IEnumerator LifetimeRoutine()
    {
        while (true)
        {
            // Active phase
            isCountingDown = false;
            SetCollidersEnabled(true);
            SetSpritesEnabled(true);
            SetParticlesEnabled(true);

            yield return new WaitForSeconds(_activeTime);

            // Respawn phase
            isCountingDown = true;
            SetCollidersEnabled(false);
            SetSpritesEnabled(false);
            SetParticlesEnabled(false);

            yield return new WaitForSeconds(_respawnTime);
        }
    }

    // ---------- Damage ----------
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCountingDown) return;

        EnemyStats enemy = collision.GetComponentInParent<EnemyStats>();
        if (enemy == null) return;

        enemy.TryTakeDamage(_damage, _piercing);
        Debug.Log(_damage);
    }

    // ---------- Component Helpers ----------
    private void CacheComponents()
    {
        if (_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider2D>(true);

        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        if (_particleSystems == null || _particleSystems.Length == 0)
            _particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    private void SetCollidersEnabled(bool state)
    {
        CacheComponents();
        foreach (var col in _colliders)
            if (col != null) col.enabled = state;
    }

    private void SetSpritesEnabled(bool state)
    {
        CacheComponents();
        foreach (var r in _spriteRenderers)
            if (r != null) r.enabled = state;
    }

    private void SetParticlesEnabled(bool state)
    {
        CacheComponents();

        foreach (var ps in _particleSystems)
        {
            if (ps == null) continue;

            ps.gameObject.SetActive(state);

            if (state)
                ps.Play();
            else
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}