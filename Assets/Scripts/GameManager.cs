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

    private bool restartIsPending = false;

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

    private int levelsCount => Database.LevelsConfiguration.LevelsData.Count - 4;

    public LevelData CurrentLevelData => CurrentLevelIndex <Database.LevelsConfiguration.LevelsData.Count ? Database.LevelsConfiguration.LevelsData[CurrentLevelIndex] :
        Database.LevelsConfiguration.LevelsData[((CurrentLevelIndex-4) % levelsCount)+4];

    public float currentCoefficient
    {
        get
        {
            if (CurrentLevelIndex < 4)
            {
                return 1;
            }
            else
            {
                return 1 + (((CurrentLevelIndex-4) / levelsCount) * Database.GameConfiguration.levelsCoefficient); 
            }
        }
    }

    public int currentLoop => (CurrentLevelIndex < Database.LevelsConfiguration.LevelsData.Count
        ? 0
        : Mathf.FloorToInt((CurrentLevelIndex - 4) / 8));

    public WaveData CurrentWaveData => CurrentLevelIndex < Database.LevelsConfiguration.LevelsData.Count ? Database.LevelsConfiguration.LevelsData[CurrentLevelIndex].WavesData[CurrentWaveIndex] :
        Database.LevelsConfiguration.LevelsData[((CurrentLevelIndex-4) % levelsCount)+4].WavesData[CurrentWaveIndex];

    public int CurrentTutorialIndex;

    public float IdleTime
    {
        get;
        private set;
    }

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

    public void ResetIdleTime()
    {
        IdleTime = 0f;
    }

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (IsInPlayMode && UIManager.Instance.State==UIState.InGame)
        {
            IdleTime += Time.deltaTime;
            if (Input.GetMouseButton(0))
            {
                ResetIdleTime();
            }
        }
        else
        {
            ResetIdleTime();
        }
    }

    void Init()
    {
        if (PlayerPrefsManager.SeenTutorial)
        {
            ResetIdleTime();
            CurrentLevelIndex = PlayerPrefsManager.Level;
            CurrentWaveIndex = 0;
            InitCurrentLevel();
            UIManager.Instance.State = UIState.MainMenu;
        }
        else
        {
            CurrentTutorialIndex = 0;
            InitCurrentTutorial();
        }
    }

    public void InitCurrentLevel()
    {
        CurrentWaveIndex=0;
        CurrentTutorialIndex = 0;
        IsInPlayMode = false;
        SoldierCellMergeManager.Instance.Init();
        SoldierCellMergeManager.Instance.ShowAllCells();

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
        int themeIndex = (CurrentLevelIndex%5)+1; 
        SpawnManager.Instance.InitMapAndTheme(CurrentLevelData.Map-1, (MapTheme)themeIndex);
        InitCurrentTutorial();
    }

    public void Restart()
    {
        if (restartIsPending)
            return;

        StartCoroutine(RestartFlow());
    }

    private IEnumerator RestartFlow()
    {
        restartIsPending = true;

        float idleTime = 0f;
        while ((SoldierCellMergeManager.Instance.IsShifting || SoldierCellMergeManager.Instance.IsMerging) && idleTime<10f)
        {
            idleTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        StartCurrentLevel();

        restartIsPending = false;
    }

    public void StartCurrentLevel()
    {
        CurrentWaveIndex=0;
        InitCurrentLevel();
        IsInPlayMode = true;
        CurrentKills = 0;
        UIManager.Instance.State = UIState.InGame;
        SoldierCellMergeManager.Instance.Init();
        SpawnManager.Instance.StartSpawningCurrentLevel();
    }

    public void InitCurrentTutorial()
    {
        if (PlayerPrefsManager.SeenTutorial && !CurrentLevelData.HasTutorial)
            return;

        if (!PlayerPrefsManager.SeenTutorial)
        {
            if (CurrentTutorialIndex < Database.TutorialConfiguration.Data.Count)
            {
                ResetIdleTime();
                IsInPlayMode = true;
                SoldierCellMergeManager.Instance.Init();
                SoldierCellMergeManager.Instance.ShowAllCells();
                SoldierCellMergeManager.Instance.InitTutorial(Database.TutorialConfiguration.Data[CurrentTutorialIndex]);
                UIManager.Instance.State = UIState.InGame;
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
                SpawnManager.Instance.InitMapAndTheme(Database.TutorialConfiguration.Map, Database.TutorialConfiguration.Theme);
                TutorialManager.Instance.Help();
            }
            else
            {
                TutorialManager.Instance.End();
                if (!PlayerPrefsManager.SeenTutorial)
                {
                    PlayerPrefsManager.SeenTutorial = true;
                    Invoke("Init", 1f);
                }
            }
        }
        else
        {
            if (CurrentTutorialIndex < CurrentLevelData.Tutorials.Count)
            {
                TutorialManager.Instance.Help();
            }
            else
            {
                TutorialManager.Instance.End();
            }
        }
    }

    private void EnemyReachedSoldierBase()
    {
        //UIManager.Instance.State = UIState.Defeat;
        Invoke(nameof(InvokeLoseUI), Database.GameConfiguration.WinLoseDialogeDelay);
        IsInPlayMode = false;
    }

    private void InvokeLoseUI()=> UIManager.Instance.State = UIState.Defeat;

    public void CurrentLevelEnded()
    {
        StartCoroutine(StartLevelEndFlow());
    }

    public void EnemyEnteredSoldierArea()
    {
        SoldierCellMergeManager.Instance.CancelConnecting();
    }

    private IEnumerator StartLevelEndFlow()
    {
        float idleTime = 0f;
        while ((SoldierCellMergeManager.Instance.IsShifting || SoldierCellMergeManager.Instance.IsMerging) && idleTime<10f)
        {
            idleTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        SoldierCellMergeManager.Instance.CancelConnecting();
        //UIManager.Instance.State = UIState.Victory;
        Invoke(nameof(InvokeWinUI), Database.GameConfiguration.WinLoseDialogeDelay);

        /*if (CurrentLevelIndex+1 < Database.LevelsConfiguration.LevelsData.Count)
        {
            CurrentLevelIndex++;
            CurrentWaveIndex=0;
        }
        else
        {
            CurrentLevelIndex=0;
            CurrentWaveIndex=0;
        }*/
        CurrentLevelIndex++;
        CurrentWaveIndex = 0;
        PlayerPrefsManager.Level = CurrentLevelIndex;
    }
    
    private void InvokeWinUI() => UIManager.Instance.State = UIState.Victory;

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
        center.y = 1.25f;
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
            Invoke(nameof(InvokeRequestShift), 0.2f);
    }

    private void InvokeRequestShift()
    {
        SoldierCellMergeManager.Instance.RequestShifting();
    }

    public void CheckIfAllZombiesDied()
    {
        if (insideEnemies.Count == 0 && outsideEnemies.Count == 0 && SpawnManager.Instance.SpawningEnded)
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
