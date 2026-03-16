using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera      _camera;
    [SerializeField] private PlayerStats _playerStats;

    [Header("Clamp")]
    [SerializeField] private float minSize = 5f;
    [SerializeField] private float maxSize = 30f;

    private void Update() => ApplySize();

    private void ApplySize()
    {
        _camera.orthographicSize = _playerStats.Area;
    }

    public void IncreaseCameraSize(StatsModifier modifier)
    {
        _playerStats.ApplyRuntimeModifier(modifier);
        ApplySize();
    }
}
