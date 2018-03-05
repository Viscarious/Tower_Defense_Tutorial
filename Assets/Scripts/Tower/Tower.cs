using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Tower : MonoBehaviour {

    [SerializeField] private float timeBetweenAttacks;
    [SerializeField] private float attackRadius;
    [SerializeField] private Projectile projectile;

    private Enemy targetEnemy = null;
    private float attackCounter;
    private bool isAttacking = false;

	// Use this for initialization
	void Start () {

        attackCounter = 0;
    }
	
	// Update is called once per frame
	void Update () {

        attackCounter -= Time.deltaTime;
        if (targetEnemy == null)
        {
            Enemy nearestEnemy = GetNearestEnemyInRange();

            if (nearestEnemy != null)
            {
                targetEnemy = nearestEnemy;
            }
        }
        else
        {
            if (attackCounter <= 0)
            {
                isAttacking = true;
                attackCounter = timeBetweenAttacks;
            }
            else
            {
                isAttacking = false;
            }

            if (Vector2.Distance(this.transform.localPosition, targetEnemy.transform.localPosition) > attackRadius || !targetEnemy.IsAlive)
            {
                targetEnemy = null;
            }
        } 
	}

    private void FixedUpdate()
    {
        if (isAttacking)
        {
            Attack();
        }
    }

    /// <summary>
    /// Attacl the nearest enemy
    /// </summary>
    public void Attack()
    {
        if(targetEnemy != null)
        {
            Projectile newProjectile = Instantiate(projectile) as Projectile;
            newProjectile.transform.localPosition = this.transform.localPosition;

            switch(newProjectile.ProjectileType)
            {
                case ProjectileType.Arrow:
                    GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Arrow);
                    break;
                case ProjectileType.Fireball:
                    GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Fireball);
                    break;
                case ProjectileType.Rock:
                    GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Rock);
                    break;
            }

            //move projectile to enemy Coroutine
            StartCoroutine(MoveProjectile(newProjectile));
        }
    }

    /// <summary>
    /// Shoot a projectile from the turret to the enemy
    /// </summary>
    /// <param name="newProjectile"></param>
    /// <returns></returns>
    IEnumerator MoveProjectile(Projectile newProjectile)
    {
        int loopNumber = 0;
        Enemy currentTarget = targetEnemy;
        //TODO Change the 0.20f into a constant
        while (Vector2.Distance(newProjectile.transform.localPosition, currentTarget.transform.localPosition) > 0.20f && newProjectile != null && targetEnemy != null)
        {
            var dir = currentTarget.transform.localPosition - this.transform.localPosition;
            var angleDirection = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            newProjectile.transform.rotation = Quaternion.AngleAxis(angleDirection, Vector3.forward);

            //TODO: Change 5.0f into a constant
            newProjectile.transform.localPosition = Vector2.MoveTowards(newProjectile.transform.localPosition, currentTarget.transform.localPosition, 5.0f * Time.deltaTime);

            //print(newProjectile.name + " loop: " + loopNumber);
            loopNumber++;
            yield return null;
        }
        
        if (newProjectile != null || currentTarget == null)
        {
            Destroy(newProjectile.gameObject);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enemy"></param>
    /// <returns></returns>
    private float GetTargetDistance(Enemy enemy)
    {
        float targetDistance = 0.0f;

        if (enemy != null)
        {
            targetDistance = Vector2.Distance(enemy.transform.localPosition, this.transform.localPosition);
        }

        return Mathf.Abs(targetDistance);
    }

    /// <summary>
    /// Get all enemies in the range of the tower
    /// </summary>
    /// <returns></returns>
    private List<Enemy> GetAllEnemiesInRange()
    {
        List<Enemy> enemiesInRange = new List<Enemy>();

        foreach(Enemy enemy in GameManager.Instance.EnemyList)
        {
            if(Vector2.Distance(this.transform.localPosition, enemy.transform.localPosition) < attackRadius && enemy.IsAlive)
            {
                enemiesInRange.Add(enemy);
            }
        }

        return enemiesInRange;
    }

    /// <summary>
    /// Get the closest enemy in range of the tower
    /// </summary>
    /// <returns></returns>
    private Enemy GetNearestEnemyInRange()
    {
        Enemy nearestEnemy = null;
        float smallestDistance = float.PositiveInfinity;

        foreach (Enemy enemy in GetAllEnemiesInRange())
        {
            if (Vector2.Distance(this.transform.localPosition, enemy.transform.localPosition) < smallestDistance)
            {
                if (enemy.IsAlive)
                {
                    smallestDistance = Vector2.Distance(this.transform.localPosition, enemy.transform.localPosition);
                    nearestEnemy = enemy;
                }
            }
        }

        return nearestEnemy;
    }
}
