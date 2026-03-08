using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RotateVisual : MonoBehaviour, IInputObserver
{
    [SerializeField] private bool  smoothRotation = true;
    [SerializeField] private float rotationSpeed  = 720f;

    private Rigidbody2D _rb;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void FixedUpdate()
    {
        if (_rb == null) return;

        Vector2 velocity = _rb.velocity;
        if (velocity.sqrMagnitude < 0.001f) return;

        float targetAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        if (smoothRotation)
        {
            float current = transform.eulerAngles.z;
            float next    = Mathf.MoveTowardsAngle(current, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, next);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
    }

    public void OnNotify(InputActions action) { }


    public void SetInitialDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
