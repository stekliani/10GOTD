using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D), typeof(MeshFilter), typeof(MeshRenderer))]
public class DonutRingRenderer : MonoBehaviour
{
    [Header("Shape")]
    [Range(16, 256)] public int segments = 64;
    public float innerRadius = 1f;
    public float outerRadius = 2f;

    [Header("Rendering")]
    public Material material;
    public string sortingLayerName = "Default";
    public int sortingOrder = -1;
    public int renderQueue = -1;

    private PolygonCollider2D poly;
    private Mesh mesh;

    private float _slowAmount = 0f;
    void Awake()
    {
        poly = GetComponent<PolygonCollider2D>();
        var mr = GetComponent<MeshRenderer>();

        var mf = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mf.mesh = mesh;

        if (material != null)
        {
            mr.material = material;
            if (renderQueue >= 0)
                mr.material.renderQueue = renderQueue;
        }

        mr.sortingLayerName = sortingLayerName;
        mr.sortingOrder = sortingOrder;

        UpdateShape();
    }

    public void SetOuterRadius(float newOuter)
    {
        outerRadius = newOuter;
        UpdateShape();
    }

    public void SetSlowAmount(float slowAmount)
    {
        _slowAmount = slowAmount;
    }

    void UpdateShape()
    {
        GenerateCollider();
        GenerateMesh();

        // Sync shader values
        if (material != null)
        {
            material.SetFloat("_InnerRadius", innerRadius);
            material.SetFloat("_OuterRadius", outerRadius);
        }
    }

    void GenerateCollider()
    {
        poly.pathCount = 2;

        Vector2[] outer = new Vector2[segments];
        Vector2[] inner = new Vector2[segments];

        float step = Mathf.PI * 2f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * step;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            outer[i] = dir * outerRadius;
            inner[i] = dir * innerRadius;
        }

        poly.SetPath(0, outer);
        poly.SetPath(1, inner);
    }

    void GenerateMesh()
    {
        mesh.Clear();

        int vertCount = segments * 2;
        Vector3[] verts = new Vector3[vertCount];
        Vector2[] uv = new Vector2[vertCount];
        int[] tris = new int[segments * 6];

        float step = Mathf.PI * 2f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * step;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Outer vertex
            verts[i * 2] = dir * outerRadius;

            // Inner vertex
            verts[i * 2 + 1] = dir * innerRadius;

            // UV (radial mapping)
            uv[i * 2] = new Vector2((float)i / segments, 1);
            uv[i * 2 + 1] = new Vector2((float)i / segments, 0);
        }

        int triIndex = 0;

        for (int i = 0; i < segments; i++)
        {
            int current = i * 2;
            int next = ((i + 1) % segments) * 2;

            // Triangle winding chosen to face the default 2D camera (looking +Z).
            tris[triIndex++] = current;
            tris[triIndex++] = current + 1;
            tris[triIndex++] = next;

            tris[triIndex++] = current + 1;
            tris[triIndex++] = next + 1;
            tris[triIndex++] = next;
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, outerRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, innerRadius);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Colliders are often on child objects; movement/slow logic lives on the root.
        var slowable = collision.GetComponentInParent<EnemyMovement>();
        if (slowable != null)
            slowable.ApplySlow(_slowAmount);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var slowable = collision.GetComponentInParent<EnemyMovement>();
        if (slowable != null)
            slowable.RemoveSlow();
    }
}