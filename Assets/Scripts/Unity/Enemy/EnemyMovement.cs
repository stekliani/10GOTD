using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMovement : MonoBehaviour, ISlowable
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private Rigidbody2D rb;

    [Header("Ranged Enemy things")]
    [Tooltip("Do not touch if this enemy is not ranged!")]
    [SerializeField] private bool isRanged = false;
    [SerializeField] private float attackRange;

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidanceRadius = 1.5f;
    [SerializeField] private float avoidanceStrength = 1.5f;

    private SpriteRenderer _spriteRenderer;
    private PlayerStats _player;
    private Vector2 _lastMoveDir;
    private EnemyAnimationController _enemyAnimationController;
    private bool _isWithinAttackRange;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _enemyAnimationController = GetComponent<EnemyAnimationController>();
    }

    private void Start()
    {
        _player = FindObjectOfType<PlayerStats>();
    }

    private void FixedUpdate()
    {
        Move();
        FlipSprite();
        ChangeAnimationState(EnemyAnimations.walking);
    }

    // ---------------- SLOW SYSTEM ----------------

    public void ApplySlow(float amount) => speed -= amount;
    public void RemoveSlow(float amount) => speed += amount;
    public void SetSpeed(float delta) => speed += delta;

    // ---------------- MOVEMENT ----------------

    private void Move()
    {
        if (_player == null)
            return;

        Vector2 toPlayer =
            (_player.transform.position - transform.position).normalized;

        Vector2 avoidance =
            EnemyMovementSystem.GetAvoidanceDirection(
                transform.position,
                avoidanceRadius,
                obstacleLayer);

        // Combine steering forces
        Vector2 moveDir = (toPlayer + avoidance * avoidanceStrength).normalized;

        if (_enemyAnimationController.GetCurrentAnimation() == EnemyAnimations.dying)
            moveDir = Vector2.zero;

        if (moveDir != Vector2.zero)
            _lastMoveDir = moveDir;


        if (isRanged)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);

            if (distanceToPlayer <= attackRange)
            {
                // Within attack range — stop moving
                moveDir = Vector2.zero;
                _isWithinAttackRange = true;
            }
            else
            {
                // Outside attack range — move toward player
                rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);
                _isWithinAttackRange = false;
            }
        }
        else
        {
            rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);
        }
    }


    public bool CheckIfRangedEnemyCanAttack()
    {
        return _isWithinAttackRange;
    }

    // ---------------- VISUALS ----------------

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

    private void ChangeAnimationState(EnemyAnimations animation)
    {
        _enemyAnimationController.ChangeAnimation(animation);
    }

    // ---------------- DEBUG ----------------

    private void OnDrawGizmos()
    {
        if (!gameObject.activeInHierarchy)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, _lastMoveDir);
        }
    }
}