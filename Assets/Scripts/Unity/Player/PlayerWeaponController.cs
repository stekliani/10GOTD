using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] private Weapon   startingWeapon;
    [SerializeField] private Weapon[] extraWeaponsArray;

    [Header("References")]
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private Transform      healingFountainSpawnPoint;

    [Header("Range Circle")]
    [SerializeField] private int   circleSegments = 64;
    [SerializeField] private float circleRadius   => _player.Area;

    private PlayerStats  _player;
    private LineRenderer _line;

    private void Awake()
    {
        _player           = GetComponent<PlayerStats>();
        _line             = gameObject.AddComponent<LineRenderer>();
        _line.loop        = true;
        _line.useWorldSpace = false;
        _line.startWidth  = 0.05f;
        _line.endWidth    = 0.05f;
        _line.material    = new Material(Shader.Find("Sprites/Default"));
        _line.startColor  = Color.green;
        _line.endColor    = Color.green;
        _line.sortingLayerName = "Default";
        _line.sortingOrder = 100;
    }

    private void Start()
    {
        if (upgradeManager == null)
            upgradeManager = FindObjectOfType<UpgradeManager>();

        SpawnWeapon(startingWeapon, active: true);

        foreach (Weapon w in extraWeaponsArray)
            SpawnWeapon(w, active: false);
    }

    private void SpawnWeapon(Weapon prefab, bool active)
    {
        Weapon instance = Instantiate(prefab, transform.position, Quaternion.identity, transform);

        if (instance is HealingFountain && healingFountainSpawnPoint != null)
        {
            instance.transform.position = healingFountainSpawnPoint.position;
        }

        instance.IsActive = active;
        instance.Initialize(_player);
        upgradeManager?.RegisterWeaponInstance(instance);
    }

    //uncoment if need to draw player firing range and call this function in update method

    //private void DrawCircle(int segments, float radius)
    //{
    //    _line.positionCount = segments;
    //    for (int i = 0; i < segments; i++)
    //    {
    //        float angle = (float)i / segments * Mathf.PI * 2f;
    //        _line.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
    //    }
    //}
}
