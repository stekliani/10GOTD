using UnityEngine;

public class Sortable : MonoBehaviour
{
    private SpriteRenderer[] _renderers;

    public bool sortingActive = true;
    public const float MIN_DISTANCE = 0.2f;

    private int _lastSortOrder;

    protected virtual void Start()
    {
        _renderers = GetComponentsInChildren<SpriteRenderer>();

        if (_renderers.Length == 0)
            Debug.LogWarning($"{name} has no SpriteRenderers for sorting.");
    }

    protected virtual void LateUpdate()
    {
        if (!sortingActive || _renderers == null || _renderers.Length == 0)
            return;

        int newSortOrder = (int)(-transform.position.y / MIN_DISTANCE);

        if (_lastSortOrder == newSortOrder)
            return;

        _lastSortOrder = newSortOrder;

        foreach (var sr in _renderers)
        {
            sr.sortingOrder = newSortOrder;
        }
    }
}