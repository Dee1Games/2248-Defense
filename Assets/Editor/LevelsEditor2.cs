using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class LevelsEditor2 : OdinMenuEditorWindow
{
    private static List<LevelData2> LevelsData;
    
    private static List<LevelEditor2> LevelEditors;

    private static OdinMenuTree tree;
    
    [MenuItem("Editor Tools/Level Editor2")]
    private static void OpenWindow()
    {
        GetWindow<LevelsEditor2>().Show();
    }

    protected override void Initialize()
    {
        BuildMenuTree();
    }

    protected override void OnGUI()
    {
        var defaultGUIColor = GUI.backgroundColor;

        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("+ Add Level",  GUILayout.Width(100)))
        {
            Database.LevelsConfiguration2.LevelsData.Add(new LevelData2());
            EditorUtility.SetDirty(Database.LevelsConfiguration2);
        }
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("- Remove Level",  GUILayout.Width(100)) && Database.LevelsConfiguration2.LevelsData.Count == tree.MenuItems.Count)
        {
            List<int> levelIndexes = new List<int>(tree.Selection.Count);
            foreach (var selectedLevel in tree.Selection)
            {
                if (!selectedLevel.Name.ToLower().Contains("level"))
                    continue;
                
                int levelIndex = int.Parse(selectedLevel.Name.ToLower().Replace("level ", "")) - 1;
                levelIndexes.Add(levelIndex);
            }
            foreach (int levelIndex in levelIndexes)
            {
                Database.LevelsConfiguration2.LevelsData.RemoveAt(levelIndex);
            }
            EditorUtility.SetDirty(Database.LevelsConfiguration2);
        }
        GUI.backgroundColor = defaultGUIColor;
        EditorGUILayout.EndHorizontal();
        
        
        if (GUI.changed)
        {
            BuildMenuTree();
            ForceMenuTreeRebuild();
        }
        base.OnGUI();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        tree = new OdinMenuTree(supportsMultiSelect: true);
        LevelEditors = new List<LevelEditor2>();
        for (int i = 0; i < Database.LevelsConfiguration2.LevelsData.Count; i++)
        {
            LevelEditor2 levelEditor2 = new LevelEditor2(i);
            LevelEditors.Add(levelEditor2);
            tree.Add("Level " + (i+1), levelEditor2);
        }
        
        return tree;
    }
}
