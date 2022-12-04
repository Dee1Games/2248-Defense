using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class LevelData2
{
    [ListDrawerSettings(ShowPaging = true, NumberOfItemsPerPage = 1, Expanded = true)]
    public List<WaveData2> WavesData;
}
