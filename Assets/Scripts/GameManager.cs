using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
    }
    
    private static GameManager _instance;

    public Action OnNewEnemySpawned, OnEnemyEntered, OnEnemyReachedSildierbase;

    public int CurrentLevelIndex
    {
        private set;
        get;
    }

    public int CurrentWaveIndex;

    private int currentKills = 0;

    public int CurrentKills
    {
        set
        {
            currentKills = value;
            UIManager.Instance.Refresh();
        }
        get
        {
            return currentKills;
        }
    }

    public bool IsInPlayMode
    {
        private set;
        get;
    }

    [HideInInspector] public List<Enemy> outsideEnemies = new List<Enemy>();
    [HideInInspector] public List<Enemy> insideEnemies = new List<Enemy>();
    //[HideInInspector] public Dictionary<(int row, int column),List<Enemy>> insideEnemies = new Dictionary<(int, int), List<Enemy>>();
    
    public LevelData CurrentLevelData => Database.LevelsConfiguration.LevelsData[CurrentLevelIndex];
    public WaveData CurrentWaveData => Database.LevelsConfiguration.LevelsData[CurrentLevelIndex].WavesData[CurrentWaveIndex];

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void OnEnable()
    {
        OnEnemyReachedSildierbase += EnemyReachedSoldierBase;
    }
    
    void OnDisable()
    {
        OnEnemyReachedSildierbase -= EnemyReachedSoldierBase;
    }

    void Start()
    {
        Init();
    }
    
    void Init()
    {
        CurrentLevelIndex = PlayerPrefsManager.Level;
        CurrentWaveIndex = 0;
        InitCurrentLevel();
        UIManager.Instance.State = UIState.MainMenu;
    }

    public void InitCurrentLevel()
    {
        IsInPlayMode = false;
        SoldierCellMergeManager.Instance.Init();

        foreach (var enemy in insideEnemies)
        {
            Destroy(enemy.gameObject);
        }
        foreach (var enemy in outsideEnemies)
        {
            Destroy(enemy.gameObject);
        }
        
        insideEnemies.Clear();
        outsideEnemies.Clear();
        SpawnManager.Instance.InitCurrentLevel();
    }

    public void StartCurrentLevel()
    {
        InitCurrentLevel();
        IsInPlayMode = true;
        CurrentKills = 0;
        UIManager.Instance.State = UIState.InGame;
        SoldierCellMergeManager.Instance.Init();
        SpawnManager.Instance.StartSpawningCurrentLevel();
    }

    private void EnemyReachedSoldierBase()
    {
        UIManager.Instance.State = UIState.Defeat;
        IsInPlayMode = false;
    }

    public void CurrentLevelEnded()
    {
        SoldierCellMergeManager.Instance.CancelConnecting();
        UIManager.Instance.State = UIState.Victory;
        if (CurrentLevelIndex+1 < Database.LevelsConfiguration.LevelsData.Count)
        {
            CurrentLevelIndex++;
            CurrentWaveIndex=0;
        }
        else
        {
            CurrentLevelIndex=0;
            CurrentWaveIndex=0;
        }
        PlayerPrefsManager.Level = CurrentLevelIndex;
    }
    
    public void CurrentWaveEnded()
    {
        GameManager.Instance.CurrentWaveIndex++;
        SpawnManager.Instance.StartSpawningCurrentWave();
    }

    public Enemy GetClosestEnemyTo(Vector3 pos)
    {
        Enemy res = null;
        float min = 999f;
        foreach (var enemy in outsideEnemies)
        {
            float dist = Vector3.Distance(pos, enemy.transform.position);
            if (dist < min)
            {
                min = dist;
                res = enemy;
            }
        }

        return res;
    }

    public void Explosion(Vector3 center, float radius,  float damage)
    {
        ParticleManager.Instance.PlayParticle(Particle_Type.Boom, center, Vector3.up);
        ParticleManager.Instance.PlayParticle(Particle_Type.Explosion, center, Vector3.up);
        List<Enemy> enemiesInExplosionRaduis = new List<Enemy>();
        foreach (var enemy in outsideEnemies)
        {
            if (Vector3.Distance(center, enemy.transform.position) < radius)
            {
                enemiesInExplosionRaduis.Add(enemy);
            }
        }
        foreach (var enemy in insideEnemies)
        {
            if (Vector3.Distance(center, enemy.transform.position) < radius)
            {
                enemiesInExplosionRaduis.Add(enemy);
            }
        }
        
        foreach (var enemy in enemiesInExplosionRaduis)
        {
            enemy.TakeDamage(damage);
        }
    }

    public void CheckIfInsideZombiesDied()
    {
        if (insideEnemies.Count == 0)
            SoldierCellMergeManager.Instance.ShiftSoldiers();
    }
    
    public void CheckIfAllZombiesDied()
    {
        if (insideEnemies.Count == 0 && outsideEnemies.Count == 0)
        {
            if (CurrentWaveIndex == CurrentLevelData.WavesData.Count - 1)
            {
                CurrentLevelEnded();
            }
            else
            {
                CurrentWaveEnded();
            }
        }
    }
}
