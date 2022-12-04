using System.Collections.Generic;
using log4net.Core;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

public class LevelEditor2 : OdinEditorWindow
{

    private int LevelIndex;
    public LevelData2 Data;
    
    public LevelEditor2(int index)
    {
        this.LevelIndex = index;
        ResetData();
    }
    
    [OnInspectorInit]
    private void ResetData()
    {
        Data = Database.LevelsConfiguration2.LevelsData[LevelIndex];
    }
    
    [OnInspectorGUI]
    private void OnGUI()
    {
        if(Data==null)
            return;
        
        GUI.backgroundColor = Color.yellow;
        if (GUILayout.Button("Overwrite"))
        {
            for (int wvw=0 ; wvw< Database.LevelsConfiguration2.LevelsData[LevelIndex].WavesData.Count ; wvw++)
            {
                List<EnemyData> enemies = new List<EnemyData>();
                foreach (EnemyData2 enemyData in Database.LevelsConfiguration2.LevelsData[LevelIndex].WavesData[wvw].EnemiesData)
                {
                    for (int i = 0; i < enemyData.N; i++)
                    {
                        EnemyData enemy = new EnemyData();
                        enemy.Type = enemyData.Type;
                        enemy.Health = enemyData.Health;
                        if (enemyData.X.x < enemyData.X.y)
                            enemy.X = Random.Range(enemyData.X.x, enemyData.X.y);
                        else
                            enemy.X = enemyData.X.x;
                        if (enemyData.Y.x < enemyData.Y.y)
                            enemy.YOffset = Random.Range(enemyData.Y.x, enemyData.Y.y);
                        else
                            enemy.YOffset = enemyData.Y.x;
                        enemy.SpawnTime = enemyData.T;
                        enemies.Add(enemy);
                    }
                }
                Database.LevelsConfiguration.LevelsData[LevelIndex].WavesData[wvw].EnemiesData = enemies;
            }
            EditorUtility.SetDirty(Database.LevelsConfiguration);
        }

        if (GUI.changed && LevelIndex<Database.LevelsConfiguration2.LevelsData.Count)
        {
            Database.LevelsConfiguration2.LevelsData[LevelIndex] = Data;
            EditorUtility.SetDirty(Database.LevelsConfiguration2);
        }
    }
    
    [OnInspectorInit]
    private void UpdateData()
    {
        ResetData();
    }
}