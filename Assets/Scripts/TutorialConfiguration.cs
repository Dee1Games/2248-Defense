using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = nameof(TutorialConfiguration), menuName = "ScriptableObjects/" + nameof(TutorialConfiguration))]
public class TutorialConfiguration : ScriptableObject
{
    public int Map;
    public MapTheme Theme;
    [ListDrawerSettings(ShowPaging = true, NumberOfItemsPerPage = 1, Expanded = true)]
    public List<TutorialData> Data;
}
