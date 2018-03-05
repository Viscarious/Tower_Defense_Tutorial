using System;
using UnityEngine;

public class Enemy : MonoBehaviour {

    private int checkpointIndex = 0;

    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float navigationUpdate;
    [SerializeField] private int healthPoints;
    [SerializeField] private int killReward;

    private Transform enemyLocation;
    private float navigationTime = 0;
    private bool isAlive = true;
    private Collider2D enemyCollider;
    private Animator animator;

    // Use this for initialization
    void Start ()
    {
        enemyLocation = GetComponent<Transform>();
        enemyCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        //TODO: Refactor this
        GameManager.Instance.RegisterEnemy(this);
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if (waypoints != null && isAlive)
        {
            navigationTime += Time.deltaTime;

            if(navigationTime > navigationUpdate)
            {
                if (checkpointIndex < waypoints.Length)
                {
                    enemyLocation.position = Vector2.MoveTowards(enemyLocation.position, waypoints[checkpointIndex].position, navigationTime);
                }
                else
                {
                    enemyLocation.position = Vector2.MoveTowards(enemyLocation.position, exitPoint.position, navigationTime);
                }

                navigationTime = 0;
            }
        }
	}

    /// <summary>
    /// Changes checkpoint destination or destroys the enemy if they've reached the finish line.
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Checkpoint")
        {
            checkpointIndex += 1;
        }
        else if (collision.tag == "Finish")
        {
            GameManager.Instance.RoundEnemiesEscaped += 1;
            GameManager.Instance.TotalEnemiesEscaped += 1;
            GameManager.Instance.DeregisterEnemy(this);
            GameManager.Instance.IsWaveOver();
        }
        else if (collision.tag == "Projectile")
        {
            Projectile newProjectile = collision.gameObject.GetComponent<Projectile>();
            EnemyHit(newProjectile.AttackStrength);
        }
    }

    /// <summary>
    /// Enemy has been hit by a projectile
    /// </summary>
    /// <param name="hitpoints"></param>
    private void EnemyHit(int hitpoints)
    {
        if(healthPoints - hitpoints > 0)
        {
            healthPoints -= hitpoints;
            GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Hit);
            animator.Play("Hurt");
        }
        else
        {
            animator.SetTrigger("Killed");
            GameManager.Instance.AudioSource.PlayOneShot(SoundManager.Instance.Death);
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;
        enemyCollider.enabled = false;
        GameManager.Instance.TotalEnemiesKilled += 1;
        GameManager.Instance.AddMoney(this.KillReward);
        GameManager.Instance.IsWaveOver();
    }

    public bool IsAlive
    {
        get
        {
            return isAlive;
        }
        
    }

    public int KillReward
    {
        get
        {
            return killReward;
        }
    }
}
