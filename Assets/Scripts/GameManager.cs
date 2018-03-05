using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public enum GameStatus
{
    Next, Play, GameOver, Win
};

public class GameManager : Singleton<GameManager> {

    [SerializeField] private int InitialFunds;
    [SerializeField] private int TotalWaves;

    [SerializeField] private GameObject spawnPoint = null;
    [SerializeField] private Enemy[] enemies;
    [SerializeField] private int maxEnemiesPerWave = 3;
    [SerializeField] private int enemiesPerSpawn;

    [SerializeField] private Text txtFunds;
    [SerializeField] private Text txtCurrentWave;
    [SerializeField] private Text txtEnemiesEscaped;
    [SerializeField] private Text txtPlayButton;
    [SerializeField] private Button btnPlayButton;

    private int currentWaveNumber = 0;
    private int currentFunds = 0;
    private int totalEnemiesEscaped = 0;
    private int roundEnemiesEscaped = 0;
    private int totalEnemiesKilled = 0;
    private int enemiesToSpawn = 0;
    private GameStatus currentState = GameStatus.Play;
    private AudioSource audioSource;

    private const float spawnDelay = 0.5f;

    public List<Enemy> EnemyList = new List<Enemy>();

    // Use this for initialization
    void Start() {

        btnPlayButton.gameObject.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        ShowMenu();
    }

    void Update()
    {
        HandleEscape();
    }

    private void ShowMenu()
    {
        switch(currentState)
        {
            case GameStatus.Play:
                txtPlayButton.text = "Click to Play";
                AudioSource.PlayOneShot(SoundManager.Instance.Gameover);
                break;
            case GameStatus.Next:
                txtPlayButton.text = "Next Wave";
                break;
            case GameStatus.Win:
                txtPlayButton.text = "Congratulations";
                break;
            case GameStatus.GameOver:
                txtPlayButton.text = "Play Again!";
                break;
        }

        btnPlayButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayButtonPressed()
    {
        switch(currentState)
        {
            case GameStatus.Next:
                currentWaveNumber += 1;
                maxEnemiesPerWave += currentWaveNumber;
                break;
            default:
                maxEnemiesPerWave = 3;
                TotalEnemiesEscaped = 0;
                CurrentFunds = 50;

                //this refers to the enemies index
                enemiesToSpawn = 0;
                TowerManager.Instance.DestroyAllTowers();
                TowerManager.Instance.ReenableBuildSites();
                txtFunds.text = CurrentFunds.ToString();
                txtEnemiesEscaped.text = "Escaped " + TotalEnemiesEscaped + "/10";
                audioSource.PlayOneShot(SoundManager.Instance.Newgame);
                break;
        }

        DestroyAllEnemies();
        TotalEnemiesKilled = 0;
        RoundEnemiesEscaped = 0;
        txtCurrentWave.text = "Wave " + (currentWaveNumber + 1);

        StartCoroutine(SpawnEnemy());

        btnPlayButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Spawn enemies at the spawn point
    /// </summary>
    IEnumerator SpawnEnemy()
    {
        if (enemiesPerSpawn > 0 && EnemyList.Count < maxEnemiesPerWave)
        {
            for (int i = 0; i < enemiesPerSpawn; i++)
            {
                if (EnemyList.Count < maxEnemiesPerWave && spawnPoint != null)
                {
                    Enemy newEnemy = Instantiate(enemies[UnityEngine.Random.Range(0, enemiesToSpawn+1)]);
                    newEnemy.transform.position = spawnPoint.transform.position;
                }
            }

            yield return new WaitForSeconds(spawnDelay);
            StartCoroutine(SpawnEnemy());
        }
    }

    public void RegisterEnemy(Enemy enemy)
    {
        EnemyList.Add(enemy);
    }

    public void DeregisterEnemy(Enemy enemy)
    {
        if (EnemyList.IndexOf(enemy) >= 0)
        {
            EnemyList.Remove(enemy);
            Destroy(enemy.gameObject);
        }
    }

    public void DestroyAllEnemies()
    {
        foreach(Enemy enemy in EnemyList)
        {
            Destroy(enemy.gameObject);
        }

        EnemyList.Clear();
    }

    public int CurrentFunds
    {
        get
        {
            return currentFunds;
        }

        set
        {
            currentFunds = value;
            txtFunds.text = currentFunds.ToString();
        }
    }

    /// <summary>
    /// Total number of enemies that made it to the exit
    /// </summary>
    public int TotalEnemiesEscaped
    {
        get
        {
            return totalEnemiesEscaped;
        }
        set
        {
            totalEnemiesEscaped = value;
        }
    }

    /// <summary>
    /// Number of enemies that made it to the exit in the most recent round
    /// </summary>
    public int RoundEnemiesEscaped
    {
        get
        {
            return roundEnemiesEscaped;
        }
        set
        {
            roundEnemiesEscaped = value;
        }
    }

    /// <summary>
    /// Number of enemies killed
    /// </summary>
    public int TotalEnemiesKilled
    {
        get
        {
            return totalEnemiesKilled;
        }
        set
        {
            totalEnemiesKilled = value;
        }
    }

    public AudioSource AudioSource
    {
        get
        {
            return audioSource;
        }
    }

    public void AddMoney(int amount)
    {
        CurrentFunds += amount;
    }

    public void SubtractMoney(int amount)
    {
        CurrentFunds -= amount;
    }

    /// <summary>
    /// Check to see if the wave is over
    /// </summary>
    public void IsWaveOver()
    {
        txtEnemiesEscaped.text = "Escaped " + TotalEnemiesEscaped + "/10";

        if((RoundEnemiesEscaped + TotalEnemiesKilled) == maxEnemiesPerWave)
        {
            if(currentWaveNumber < (enemies.Length-1))
            {
                enemiesToSpawn += 1;
            }

            SetCurrentGameState();
            ShowMenu();
        }
    }

    public void SetCurrentGameState()
    {
        //TODO: Make the 10 a constant variable
        if (TotalEnemiesEscaped >= 10)
        {
            currentState = GameStatus.GameOver;
        }
        else if(currentWaveNumber == 0 && (TotalEnemiesKilled + RoundEnemiesEscaped) == 0)
        {
            currentState = GameStatus.Play;
        }
        else if (currentWaveNumber >= TotalWaves)
        {
            currentState = GameStatus.Win;
        }
        else
        {
            currentState = GameStatus.Next;
        }
    }

    private void HandleEscape()
    {
        if(Input.GetMouseButton(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            TowerManager.Instance.DisableDragSprite();
            TowerManager.Instance.towerButtonPressed = null;
        }
    }
}
