using System.Collections.Generic;
using UnityEngine;

public class FireballWeapon : Weapon
{
    private const int MAX_FIREBALLS = 3;

    [SerializeField] private GameObject _fireballHolder;
    [Tooltip("Base orbit rate in degrees/sec when weapon Speed is 100 and Projectile Speed bonus is 0.")]
    [SerializeField] private float _orbitSpeed = 90f;

    private float _orbitRadius;
    private readonly List<Fireball> _allFireballs = new List<Fireball>();
    private float _currentAngle = 0f;
    private int _lastActiveCount = -1;
    private PlayerStats _player;

    // Init
    protected new void Awake()
    {
        base.Awake();

        _player = FindObjectOfType<PlayerStats>();

        // Parent fireballs to this weapon so they never leave stale references.
        GameObject holderInstance = Instantiate(_fireballHolder, transform);
        holderInstance.transform.localPosition = Vector3.zero;
        CollectFireballs(holderInstance);

        foreach (Fireball fb in _allFireballs)
        {
            if (fb == null) continue;
            fb.Initialize(data.Lifetime, data.Interval);
            fb.gameObject.SetActive(false);
        }
    }

    protected override void Update()
    {
        if (!IsActive) return;

        int desiredCount = Mathf.Clamp(data.Amount, 0, MAX_FIREBALLS);

        // Active count changed -> cancel all, restart fresh.
        if (desiredCount != _lastActiveCount)
        {
            ApplyFireballCount(desiredCount);
            _lastActiveCount = desiredCount;
        }

        if (desiredCount == 0) return;
        if (_player == null) return;

        // Angular speed: base * weapon Speed (100 = 1x) * same ProjectileSpeed scaling as projectiles.
        float weaponSpeedFactor = data.Speed / 100f;
        float projectileSpeedFactor = 1f + _player.ProjectileSpeed / 100f;
        float orbitDegPerSec = _orbitSpeed * weaponSpeedFactor * projectileSpeedFactor;

        _currentAngle = Mathf.Repeat(_currentAngle + orbitDegPerSec * Time.deltaTime, 360f);

        // Even spacing: equal slices, shared rotation offset.
        float slice = 360f / desiredCount;
        int activeIndex = 0;

        for (int i = 0; i < _allFireballs.Count; i++)
        {
            Fireball fb = _allFireballs[i];
            if (fb == null || !fb.gameObject.activeSelf) continue;

            float angle = _currentAngle + (activeIndex * slice);
            _orbitRadius = Mathf.Clamp(_player.Area, 1, 5);
            fb.SetOrbitParams(angle, _orbitRadius);
            fb.UpdatePosition(_player.transform);

            activeIndex++;
        }
    }

    protected override void Fire() { }

    private void ApplyFireballCount(int count)
    {
        for (int i = 0; i < _allFireballs.Count; i++)
        {
            Fireball fb = _allFireballs[i];
            if (fb == null) continue;

            if (i < count)
            {
                fb.CancelAndRestore();

                if (!fb.gameObject.activeSelf)
                    fb.gameObject.SetActive(true);

                fb.SetDamage(GetCurrentFireballDamage(), _player != null ? _player.Piercing : 0f);
                fb.StartLifetime();
            }
            else
            {
                fb.CancelAndRestore();
                fb.gameObject.SetActive(false);
            }
        }
    }

    private float GetCurrentFireballDamage()
    {
        if (_player == null) return data.Damage;
        float damageMultiplier = 1f + _player.DamageBoost / 100f;
        return data.Damage * damageMultiplier;
    }

    // Helpers
    private void CollectFireballs(GameObject holderRoot)
    {
        _allFireballs.Clear();
        foreach (Fireball fb in holderRoot.GetComponentsInChildren<Fireball>(includeInactive: true))
            _allFireballs.Add(fb);
    }
}

