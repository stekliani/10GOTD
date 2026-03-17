using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera      _camera;
    [SerializeField] private PlayerStats _playerStats;

    [Header("Clamp")]
    [SerializeField] private float minSize;
    [SerializeField] private float maxSize;

    private void Update() => ApplySize();

    private void ApplySize()
    {
        float area = _playerStats.Area * 2f;
        float min = minSize;
        float max = maxSize;
        float clamped = Mathf.Clamp(area, min, max);

        Debug.Log($"Area: {area}, min: {min}, max: {max}, clamped: {clamped}");

        _camera.orthographicSize = clamped;
    }

    public void IncreaseCameraSize(StatsModifier modifier)
    {
        _playerStats.ApplyRuntimeModifier(modifier);
        ApplySize();
    }
}
