using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelsConfiguration), menuName = "ScriptableObjects/" + nameof(LevelsConfiguration))]
public class LevelsConfiguration : ScriptableObject
{
    [ListDrawerSettings(ShowPaging = true, NumberOfItemsPerPage = 1, Expanded = true)]
    public List<LevelData> LevelsData;
}
