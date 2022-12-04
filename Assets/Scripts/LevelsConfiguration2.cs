using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(LevelsConfiguration2), menuName = "ScriptableObjects/" + nameof(LevelsConfiguration2))]
public class LevelsConfiguration2 : ScriptableObject
{
    [ListDrawerSettings(ShowPaging = true, NumberOfItemsPerPage = 1, Expanded = true)]
    public List<LevelData2> LevelsData;
}