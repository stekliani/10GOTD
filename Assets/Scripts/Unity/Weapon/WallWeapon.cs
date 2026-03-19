using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallWeapon : Weapon
{
    [SerializeField] private GameObject _wallsHolder;
    private List<Wall> _walls = new List<Wall>();
    private float _maxHealth;

    protected new void Awake()
    {
        base.Awake();
        GetWalls();

        _maxHealth = data.Health;

        foreach (Wall wall in _walls)
        {
            wall.InitializeHealth(_maxHealth);
            wall.respawnTime = GetModifiedInterval();
            wall.gameObject.SetActive(false);
        }
    }

    protected override void Fire() { }

    protected override void Update()
    {
        if (!IsActive) return;

        foreach (Wall wall in _walls)
        {
            // Enable walls when weapon becomes active (and wall isn't respawning)
            if (!wall.gameObject.activeSelf && !wall.isCountingDown)
            {
                wall.gameObject.SetActive(true);
            }

            // Check if a living wall has run out of health
            if (wall.gameObject.activeSelf && wall.currentHealth <= 0)
            {
                wall.StartRespawn();
            }
        }
    }

    private void GetWalls()
    {
        // GetComponentsInChildren works on components, not GameObjects
        foreach (Wall wall in _wallsHolder.GetComponentsInChildren<Wall>(includeInactive: true))
        {
            _walls.Add(wall);
        }
    }
}