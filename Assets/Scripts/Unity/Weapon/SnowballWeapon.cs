using System.Collections.Generic;
using UnityEngine;

public class SnowballWeapon : Weapon
{
    private const int MAX_SNOWBALLS = 3;

    [SerializeField] private GameObject _snowballHolder;
    [SerializeField] private float _orbitSpeed = 90f; // degrees per second
    private float _orbitRadius;

    private readonly List<Snowball> _allSnowballs = new List<Snowball>();
    private float _currentAngle = 0f;
    private int _lastActiveCount = -1;
    private PlayerStats _player;
    // Init
    protected new void Awake()
    {
        base.Awake();
        _player = FindObjectOfType<PlayerStats>();
        // Parent to this weapon so snowballs are destroyed with the weapon and never leave stale refs.
        GameObject holderInstance = Instantiate(_snowballHolder, transform);
        holderInstance.transform.localPosition = Vector3.zero;
        CollectSnowballs(holderInstance);

        foreach (Snowball sb in _allSnowballs)
        {
            if (sb == null) continue;
            sb.Initialize(data.Lifetime, data.Interval);
            sb.gameObject.SetActive(false);
        }
    }

    // Per-frame logic
    protected override void Update()
    {
        if (!IsActive) return;

        int desiredCount = Mathf.Clamp(data.Amount, 0, MAX_SNOWBALLS);

        // Active count changed -> cancel all, restart fresh
        if (desiredCount != _lastActiveCount)
        {
            ApplySnowballCount(desiredCount);
            _lastActiveCount = desiredCount;
        }

        if (desiredCount == 0) return;

        // Advance orbit angle
        _currentAngle += _orbitSpeed * Time.deltaTime;
        if (_currentAngle >= 360f) _currentAngle -= 360f;

        // Position active snowballs evenly around orbit
        float slice = 360f / desiredCount;
        int activeIndex = 0;

        for (int i = 0; i < _allSnowballs.Count; i++)
        {
            Snowball sb = _allSnowballs[i];
            if (sb == null || !sb.gameObject.activeSelf) continue;

            float angle = _currentAngle + (activeIndex * slice);
            _orbitRadius = Mathf.Clamp(_player.Area, 1, 5);
            sb.SetOrbitParams(angle, _orbitRadius);
            sb.UpdatePosition(_player.transform);

            activeIndex++;
        }
    }

    protected override void Fire() { }

    // Count management
    private void ApplySnowballCount(int count)
    {
        for (int i = 0; i < _allSnowballs.Count; i++)
        {
            Snowball sb = _allSnowballs[i];
            if (sb == null)
                continue;

            if (i < count)
            {
                // Cancel any running coroutine so the snowball is immediately restored
                sb.CancelAndRestore();

                if (!sb.gameObject.activeSelf)
                    sb.gameObject.SetActive(true);

                // All snowballs restart their lifetime cycle together
                sb.StartLifetime();
            }
            else
            {
                sb.CancelAndRestore();
                sb.gameObject.SetActive(false);
            }
        }
    }

    // Called by Snowball when it finishes its respawn phase.
    // Once every active snowball has come back, clear the shared flag.
    public void OnSnowballRespawned(Snowball snowball)
    {
        foreach (Snowball sb in _allSnowballs)
        {
            if (sb == null) continue;
            if (sb.gameObject.activeSelf && sb.isCountingDown)
            {
                break;
            }
        }
    }

    // Helpers
    private void CollectSnowballs(GameObject holderRoot)
    {
        _allSnowballs.Clear();
        foreach (Snowball sb in holderRoot.GetComponentsInChildren<Snowball>(includeInactive: true))
            _allSnowballs.Add(sb);
    }
}