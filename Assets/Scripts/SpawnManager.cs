using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public bool SpawningEnded;

    [SerializeField] private ObjectPool simpleEnemyPool;
    [SerializeField] private ObjectPool giantEnemyPool;
    [SerializeField] private Transform enemiesContainer;
    [SerializeField] private List<Map> maps;
    [SerializeField] private List<GameObject> themes;
    [SerializeField] private Transform spawnPos;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void StartSpawningCurrentLevel()
    {
        SpawningEnded = false;
        StartCoroutine(SpawnCurrentLevel());
    }

    public void StartSpawningCurrentWave()
    {
        SpawningEnded = false;
        StartCoroutine(SpawnCurrentWave());
    }

    public void InitMapAndTheme(int map, MapTheme theme)
    {
        foreach (Map _map in maps)
        {
            _map.gameObject.SetActive(false);
        }
        foreach (GameObject _theme in themes)
        {
            _theme.SetActive(false);
        }
        
        maps[map].gameObject.SetActive(true);
        maps[map].ActivateMapTheme(theme);
        themes[(int)theme].SetActive(true);
    }

    private IEnumerator SpawnCurrentLevel()
    {
        SpawningEnded = false;
        LevelData levelData = GameManager.Instance.CurrentLevelData;
        yield return new WaitForSeconds(levelData.WaitTimeBeforeSpawnLevel);
        StartCoroutine(SpawnCurrentWave());
    }

    private IEnumerator SpawnCurrentWave()
    {
        SpawningEnded = false;
        while (GameManager.Instance.outsideEnemies.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        WaveData waveData = GameManager.Instance.CurrentWaveData;
        yield return new WaitForSeconds(waveData.WaitTimeBeforeSpawnWave);
        int spawnedEnemiesCount = 0;
        List<EnemyData> enemiesToSpawn = new List<EnemyData>(waveData.EnemiesData.Count);
        for(int i=0 ; i<waveData.EnemiesData.Count; i++)
        {
            enemiesToSpawn.Add(waveData.EnemiesData[i]);
        }
        enemiesToSpawn = enemiesToSpawn.OrderBy(o=>o.SpawnTime).ToList();
        float spawnStartTime = Time.timeSinceLevelLoad;
        while (spawnedEnemiesCount < enemiesToSpawn.Count)
        {
            if (Time.timeSinceLevelLoad - spawnStartTime < enemiesToSpawn[spawnedEnemiesCount].SpawnTime)
            {
                yield return new WaitForEndOfFrame();
            }
            else
            {
                SpawnEnemy(enemiesToSpawn[spawnedEnemiesCount]);
                spawnedEnemiesCount++;
            }
        }
        SpawningEnded = true;
    }

    private void SpawnEnemy(EnemyData enemyData)
    {
        Enemy enemy = (enemyData.Type==EnemyType.SimpleEnemy?simpleEnemyPool:giantEnemyPool).Spawn(enemiesContainer).GetComponent<Enemy>();
        enemy.Init(enemyData);
        enemy.transform.position = new Vector3(enemyData.X, 0f, spawnPos.position.z+enemy.Data.YOffset);
        GameManager.Instance.outsideEnemies.Add(enemy);
        GameManager.Instance.OnNewEnemySpawned.Invoke();
    }
}
