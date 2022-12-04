using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

[System.Serializable]
public class TutorialData
{
    public TutorialPath Path;
    [TableList(AlwaysExpanded = true, DrawScrollView = false)]
    public List<CellData> CellsData;
}
