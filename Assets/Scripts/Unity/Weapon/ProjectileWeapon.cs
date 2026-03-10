using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float      firingRange => player.Area;

    [Header("Weapon SFX Type")]
    [SerializeField] private SoundActions fireSFX;

    [Header("Projectile Settings")]
    [SerializeField] private bool isHomingProjectile;
    [SerializeField] private int  projectileAmount => player.Amount + 1;


    protected override void Update()
    {
        if (player == null) return;
        timer -= Time.deltaTime;

        if (timer <= 0f && CanFire())
        {
            Fire();
            timer = GetModifiedInterval();
        }
    }

    protected override void Fire()
    {
        List<EnemyStats> enemiesInRange = GetAliveEnemiesInRange();
        int enemyCount = enemiesInRange.Count;

        if (enemyCount == 0) return;

        // SINGLE PROJECTILE
        if (projectileAmount == 1)
        {
            SpawnProjectile(GetClosest(enemiesInRange));
            SoundEventBus.Raise(fireSFX);
            return;
        }

        // MULTI PROJECTILE
        EnemyStats closest = GetClosest(enemiesInRange);

        // Case 1: projectileAmount <= enemyCount -> unique random targets
        if (projectileAmount <= enemyCount)
        {
            // Shuffle first N elements using partial Fisher–Yates
            for (int i = 0; i < projectileAmount; i++)
            {
                int randomIndex = Random.Range(i, enemyCount);

                // swap
                EnemyStats temp = enemiesInRange[i];
                enemiesInRange[i] = enemiesInRange[randomIndex];
                enemiesInRange[randomIndex] = temp;

                SpawnProjectile(enemiesInRange[i]);
                SoundEventBus.Raise(fireSFX);
            }
        }
        // Case 2: projectileAmount > enemyCount
        else
        {
            // Fire once at every enemy (unique)
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnProjectile(enemiesInRange[i]);
                SoundEventBus.Raise(fireSFX);
            }

            // Remaining shots go to closest enemy
            int remaining = (int)projectileAmount - enemyCount;

            for (int i = 0; i < remaining; i++)
            {
                SpawnProjectile(closest);
                SoundEventBus.Raise(fireSFX);
            }
        }
    }

    private void SpawnProjectile(EnemyStats target)
    {
        if (target == null) return;

        Vector2 direction =
            (target.transform.position - transform.position).normalized;

        var go = MainPoolManager.Instance.Get(projectilePrefab);
        Projectile proj = go.GetComponent<Projectile>();
        proj.transform.position = transform.position;
        proj.transform.rotation = Quaternion.identity;

        proj.Initialize(direction, player, data, isHomingProjectile, target, this);

        proj.GetRotateVisual()?.SetInitialDirection(direction);
    }

    private List<EnemyStats> GetAliveEnemiesInRange()
    {
        return EnemyQuery.GetAliveEnemiesInRange(
            (Vector2)transform.position,
            firingRange
        );
    }

    private EnemyStats GetClosest(List<EnemyStats> enemies)
    {
        return EnemyQuery.GetClosestEnemy(
            (Vector2)transform.position,
            enemies
        );
    }

    public EnemyStats GetClosestEnemyForProjectile()
    {
        return EnemyQuery.GetClosestAliveEnemyInRange(
            (Vector2)transform.position,
            firingRange
        );
    }
}
