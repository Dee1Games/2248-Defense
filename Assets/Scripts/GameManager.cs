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

    public int CurrentLevelIndex
    {
        private set;
        get;
    }

    public int CurrentWaveIndex
    {
        private set;
        get;
    }

    [HideInInspector] public List<Enemy> CurrentEnemies = new List<Enemy>();
    
    public LevelData CurrentLevelData => Database.LevelsConfiguration.LevelsData[CurrentLevelIndex];
    public WaveData CurrentWaveData => Database.LevelsConfiguration.LevelsData[CurrentLevelIndex].WavesData[CurrentWaveIndex];

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        CurrentLevelIndex = PlayerPrefsManager.Level;
        CurrentWaveIndex = 0;
        SpawnManager.Instance.StartSpawningLevel(CurrentLevelIndex);
    }
}
