using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class DonutCollider2D : MonoBehaviour
{
    public int segments = 64;        // how smooth the circle is
    public float innerRadius = 1f;   // inner circle radius
    public float outerRadius = 2f;   // outer circle radius (thickness = outer - inner)

    private PolygonCollider2D poly;

    void Start()
    {
        SetRadius(outerRadius);
    }

    public void SetRadius(float outerRadius)
    {
        poly = GetComponent<PolygonCollider2D>();
        poly.isTrigger = true;

        // PolygonCollider2D supports multiple paths
        poly.pathCount = 2; // outer + inner (hole)

        Vector2[] outer = new Vector2[segments];
        Vector2[] inner = new Vector2[segments];

        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            outer[i] = new Vector2(cos, sin) * outerRadius;
            inner[i] = new Vector2(cos, sin) * innerRadius;
        }

        poly.SetPath(0, outer); // outer ring
        poly.SetPath(1, inner); // hole (inner)
    }
}