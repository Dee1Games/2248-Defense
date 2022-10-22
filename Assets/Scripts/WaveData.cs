using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class WaveData
{
    public float WaitTimeBeforeSpawnWave;
    public float EnemySpawnRate;
    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    public List<EnemyData> EnemiesData;
}
