using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

[System.Serializable]
public class WaveData
{
    public float WaitTimeBeforeSpawnWave;
    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    public List<EnemyData> EnemiesData;
}
