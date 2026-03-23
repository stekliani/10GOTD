using System.Collections;
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

    // Orbit state - set by FireballWeapon each frame
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

    // Orbit helpers - called by FireballWeapon
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

    // Setup - called by FireballWeapon on init and after count changes
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

    // Begin the active->respawn->active loop
    public void StartLifetime()
    {
        if (_lifetimeCoroutine != null)
            StopCoroutine(_lifetimeCoroutine);

        _lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
    }

    // Stop any running coroutine and immediately make this fireball active/visible.
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

            // Respawn phase (despawn)
            yield return RespawnRoutine();
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isCountingDown = true;
        SetCollidersEnabled(false);
        SetSpritesEnabled(false);
        SetParticlesEnabled(false);

        yield return new WaitForSeconds(_respawnTime);
    }

    private IEnumerator HitRespawnRoutine()
    {
        // Already despawned when this starts.
        yield return new WaitForSeconds(_respawnTime);

        // Back to active->respawn loop
        while (true)
        {
            isCountingDown = false;
            SetCollidersEnabled(true);
            SetSpritesEnabled(true);
            SetParticlesEnabled(true);

            yield return new WaitForSeconds(_activeTime);

            isCountingDown = true;
            SetCollidersEnabled(false);
            SetSpritesEnabled(false);
            SetParticlesEnabled(false);

            yield return new WaitForSeconds(_respawnTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCountingDown) return; // ignore when despawned

        EnemyStats enemy = collision.GetComponentInParent<EnemyStats>();
        if (enemy == null) return;

        // Deals damage (piercing behavior matches your EnemyStats implementation).
        enemy.TryTakeDamage(_damage, _piercing);

        // "Destroy self" meaning: despawn immediately and come back after respawn.
        if (_lifetimeCoroutine != null)
        {
            StopCoroutine(_lifetimeCoroutine);
            _lifetimeCoroutine = null;
        }

        isCountingDown = true;
        SetCollidersEnabled(false);
        SetSpritesEnabled(false);
        SetParticlesEnabled(false);

        _lifetimeCoroutine = StartCoroutine(HitRespawnRoutine());
    }

    // Component helpers
    private void CacheComponents()
    {
        if (_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider2D>(includeInactive: true);

        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        if (_particleSystems == null || _particleSystems.Length == 0)
            _particleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
    }

    private void SetCollidersEnabled(bool state)
    {
        CacheComponents();
        if (_colliders == null) return;
        foreach (Collider2D col in _colliders)
            if (col != null) col.enabled = state;
    }

    private void SetSpritesEnabled(bool state)
    {
        CacheComponents();
        if (_spriteRenderers == null) return;
        foreach (SpriteRenderer r in _spriteRenderers)
            if (r != null) r.enabled = state;
    }

    private void SetParticlesEnabled(bool state)
    {
        CacheComponents();
        if (_particleSystems == null) return;

        foreach (ParticleSystem ps in _particleSystems)
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

