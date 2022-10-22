using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private static SpawnManager _instance;

    [SerializeField] private ObjectPool simpleEnemyPool;
    [SerializeField] private ObjectPool giantEnemyPool;
    [SerializeField] private Transform enemiesContainer;
    [SerializeField] private float spawnOffset;
    [SerializeField] private List<GameObject> maps;

    private int currentLevelIndex;
    private int currentWaveIndex;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void StartSpawningLevel(int levelIndex)
    {
        currentLevelIndex = levelIndex;
        StartCoroutine(SpawnCurrentLevel());
    }

    private IEnumerator SpawnCurrentLevel()
    {
        foreach (GameObject map in maps)
        {
            map.SetActive(false);
        }
        currentWaveIndex = 0;
        LevelData levelData = Database.LevelsConfiguration.LevelsData[currentLevelIndex];
        maps[levelData.Map].SetActive(true);
        yield return new WaitForSeconds(levelData.WaitTimeBeforeSpawnLevel);
        StartCoroutine(SpawnCurrentWave());
    }

    private IEnumerator SpawnCurrentWave()
    {
        WaveData waveData = Database.LevelsConfiguration.LevelsData[currentLevelIndex].WavesData[currentWaveIndex];
        yield return new WaitForSeconds(waveData.WaitTimeBeforeSpawnWave);
        int spawnedEnemiesCount = 0;
        while (spawnedEnemiesCount < waveData.EnemiesData.Count)
        {
            yield return new WaitForSeconds(waveData.EnemySpawnRate);
            SpawnEnemy(waveData.EnemiesData[spawnedEnemiesCount]);
            spawnedEnemiesCount++;
        }

        if (currentWaveIndex+1 < Database.LevelsConfiguration.LevelsData[currentLevelIndex].WavesData.Count)
        {
            currentWaveIndex++;
            StartCoroutine(SpawnCurrentWave());
        }
        else
        {
            if (currentLevelIndex+1 < Database.LevelsConfiguration.LevelsData.Count)
            {
                currentLevelIndex++;
                currentWaveIndex=0;
            }
            else
            {
                currentLevelIndex=0;
                currentWaveIndex=0;
            }
            PlayerPrefsManager.Level = currentLevelIndex;
            StartCoroutine(SpawnCurrentLevel());
        }
    }

    private void SpawnEnemy(EnemyData enemyData)
    {
        Enemy enemy = (enemyData.Type==EnemyType.SimpleEnemy?simpleEnemyPool:giantEnemyPool).Spawn(enemiesContainer).GetComponent<Enemy>();
        enemy.Init(enemyData);
        enemy.transform.position = new Vector3(enemyData.X, 0f, spawnOffset);
        GameManager.Instance.CurrentEnemies.Add(enemy);
    }
}
