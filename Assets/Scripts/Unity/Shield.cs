using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour,IEnemyTarget
{

    [SerializeField] private float maxHealthPoints;
    private float _currentHealth;

    private void Awake()
    {
        _currentHealth = maxHealthPoints;
    }

    private void Update()
    {
        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        Debug.Log(_currentHealth);
    }
}
