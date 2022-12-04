using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

[System.Serializable]
public class WaveData2
{
    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    public List<EnemyData2> EnemiesData;
}
