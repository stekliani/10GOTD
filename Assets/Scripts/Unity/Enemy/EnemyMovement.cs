using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour, ISlowable, IFreezable
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private Rigidbody2D rb;

    [Header("Ranged Enemy")]
    [Tooltip("Leave false for melee enemies.")]
    [SerializeField] private bool isRanged;
    [SerializeField] private float attackRange;

    [Header("Grid Pathfinding")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float pathRefreshInterval = 0.3f; // seconds between AStar recalculations
    [SerializeField] private float waypointReachRadius = 0.1f; // distance to consider a waypoint reached

    // private state

    private SpriteRenderer _spriteRenderer;
    private PlayerStats _player;
    private EnemyAnimationController _enemyAnimationController;
    private EnemyStats _enemyStats;

    private List<Vector2> _path = new();
    private int _pathIndex;
    private float _nextPathTime;
    private Vector2 _lastMoveDir;
    private bool _isWithinAttackRange;
    private float _freezeTime;
    private float _baseSpeed;
    private float _waveSpeedMultiplier = 1f;
    private float _currentSpeed;

    //public state
    public bool isFrozen => _freezeTime > 0f;


    // lifecycle

    private void Awake()
    {
        _baseSpeed = speed;
        _currentSpeed = _baseSpeed;
    }

    private void OnEnable()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _enemyAnimationController = GetComponent<EnemyAnimationController>();
        _enemyStats = GetComponent<EnemyStats>();
        _enemyAnimationController.ChangeAnimation(EnemyAnimations.walking);
        _currentSpeed = _baseSpeed * _waveSpeedMultiplier;
    }

    private void Start()
    {
        _player = FindObjectOfType<PlayerStats>();
    }

    private void Update()
    {
        _freezeTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        RefreshPathIfDue();
        Move();
        FlipSprite();
        Debug.Log("currentSpeed:" + _currentSpeed);
    }

    // ISlowable

    public void ApplySlow(float amount)
    {
        float percent = amount / 100f;
        _currentSpeed -= _currentSpeed * percent;
    }
    public void RemoveSlow() => _currentSpeed = _baseSpeed * _waveSpeedMultiplier;

    //pathfinding

    private void RefreshPathIfDue()
    {
        if (_player == null || Time.time < _nextPathTime)
            return;

        _nextPathTime = Time.time + pathRefreshInterval;

        _path = GridPathfinder.FindPath(
            transform.position,
            _player.transform.position,
            IsWalkable);

        _pathIndex = 0;
    }

    /// <summary>
    /// A cell is walkable when no obstacle collider overlaps its centre.
    /// Only called during AStar search, not every frame.
    /// </summary>
    private bool IsWalkable(Vector2Int cell)
    {
        Vector2 centre = GridPathfinder.CellToWorld(cell);
        float boxSize = GridPathfinder.CellSize * 0.9f; // slightly inset to avoid edge grazing
        return !Physics2D.OverlapBox(centre, Vector2.one * boxSize, 0f, obstacleLayer);
    }

    // movement

    private void Move()
    {
        if (isFrozen) { return; }
        if (_player == null || !_enemyStats.CheckIfAlive()) {  return; }

        // Ranged enemies stop once inside attack range
        if (isRanged)
        {
            float dist = Vector2.Distance(transform.position, _player.transform.position);
            _isWithinAttackRange = dist <= attackRange;
            if (_isWithinAttackRange) { return; }
        }

        if (_path == null || _pathIndex >= _path.Count) {  return; }

        Vector2 target = _path[_pathIndex];
        Vector2 toTarget = target - (Vector2)transform.position;
        float distance = toTarget.magnitude;

        // Waypoint reached — advance to next
        if (distance <= waypointReachRadius)
        {
            _pathIndex++;
            return;
        }

        Vector2 moveDir = toTarget.normalized;
        _lastMoveDir = moveDir;
        rb.MovePosition(rb.position + moveDir * _currentSpeed * Time.fixedDeltaTime);
    }

    public bool CheckIfRangedEnemyCanAttack() => _isWithinAttackRange;

    // visuals

    private void FlipSprite()
    {
        if (_player == null || _spriteRenderer == null)
            return;

        bool faceLeft = _player.transform.position.x < transform.position.x;
        Transform visuals = _spriteRenderer.transform.parent;

        if (visuals == null)
            return;

        Vector3 scale = visuals.localScale;
        scale.x = Mathf.Abs(scale.x) * (faceLeft ? -1 : 1);
        visuals.localScale = scale;
    }

    // debug gizmos

    private void OnDrawGizmos()
    {
        if (!gameObject.activeInHierarchy)
            return;

        // Draw the current AStar path
        if (_path != null && _path.Count > 0)
        {
            Gizmos.color = Color.cyan;
            Vector3 prev = transform.position;
            for (int i = _pathIndex; i < _path.Count; i++)
            {
                Gizmos.DrawLine(prev, _path[i]);
                Gizmos.DrawWireSphere(_path[i], 0.1f);
                prev = _path[i];
            }
        }

        // Draw current movement direction
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, _lastMoveDir);
        }
    }

    public void Freeze(float freezeTime)
    {
        _freezeTime = 0f;
        _freezeTime = freezeTime;
    }

    public void SetWaveSpeedMultiplier(float multiplier)
    {
        _waveSpeedMultiplier = Mathf.Max(0f, multiplier);
        _currentSpeed = _baseSpeed * _waveSpeedMultiplier;
    }
}