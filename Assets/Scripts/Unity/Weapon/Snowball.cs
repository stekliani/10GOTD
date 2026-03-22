using System.Collections;
using UnityEngine;

public class Snowball : MonoBehaviour
{

    [SerializeField] private float freezeTime;
    private Collider2D[] _colliders;
    private SpriteRenderer[] _spriteRenderers;
    private ParticleSystem[] _particleSystems;

    public bool isCountingDown = false;
    private float _activeTime;
    private float _respawnTime;
    private Coroutine _lifetimeCoroutine;

    private SnowballWeapon _owner;

    // Orbit state - set by SnowballWeapon each frame
    private float _orbitAngle;
    private float _orbitRadius;

    private void Awake()
    {
        _owner = GetComponentInParent<SnowballWeapon>();
    }

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

    // Orbit helpers - called by SnowballWeapon
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

    // Setup - called by SnowballWeapon on init and after count changes
    public void Initialize(float activeTime, float respawnTime)
    {
        _activeTime = activeTime;
        _respawnTime = respawnTime;
    }

    // Begin the active->respawn->active loop
    public void StartLifetime()
    {
        if (_lifetimeCoroutine != null)
            StopCoroutine(_lifetimeCoroutine);
        _lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
    }

    // Stop any running coroutine and immediately make this snowball active/visible
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

        // Notify owner so it can clear _isRespawning when all snowballs are done
        _owner.OnSnowballRespawned(this);

        isCountingDown = false;
        SetCollidersEnabled(true);
        SetSpritesEnabled(true);
        SetParticlesEnabled(true);

        // Loop into the next cycle
        _lifetimeCoroutine = StartCoroutine(LifetimeRoutine());
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
        if (_colliders == null || _colliders.Length == 0)
            _colliders = GetComponentsInChildren<Collider2D>(includeInactive: true);
        foreach (Collider2D col in _colliders)
            if (col != null) col.enabled = state;
    }

    private void SetSpritesEnabled(bool state)
    {
        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        foreach (SpriteRenderer r in _spriteRenderers)
            if (r != null) r.enabled = state;
    }

    private void SetParticlesEnabled(bool state)
    {
        if (_particleSystems == null || _particleSystems.Length == 0)
            _particleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyMovement enemy = collision.GetComponentInParent<EnemyMovement>();
        if (enemy != null)
        {
            if (enemy.TryGetComponent(out IFreezable freezable))
            {
                if (!freezable.isFrozen)
                {
                    freezable.Freeze(freezeTime);
                }
            }
        }
    }
}