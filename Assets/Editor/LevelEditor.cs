using log4net.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

public class LevelEditor : OdinEditorWindow
{

    private int LevelIndex;
    
    [TableMatrix()]
    public int[,] Matrix = new int[5, 4];
    [Space(30)]
    public LevelData Data;
    
    public LevelEditor(int index)
    {
        this.LevelIndex = index;
        ResetData();
    }
    
    [OnInspectorInit]
    private void ResetData()
    {
        Data = Database.LevelsConfiguration.LevelsData[LevelIndex];
        Matrix = Database.LevelsConfiguration.LevelsData[LevelIndex].GetMatrix();
    }
    
    [OnInspectorGUI]
    private void OnGUI()
    {
        if(Data==null || Matrix==null)
            return;

        if (GUI.changed && LevelIndex<Database.LevelsConfiguration.LevelsData.Count)
        {
            Database.LevelsConfiguration.LevelsData[LevelIndex] = Data;
            Database.LevelsConfiguration.LevelsData[LevelIndex].SetMatrix(Matrix);
        }
    }
    
    [OnInspectorInit]
    private void UpdateData()
    {
        ResetData();
    }
}