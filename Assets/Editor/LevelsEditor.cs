using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class LevelsEditor : OdinMenuEditorWindow
{
    private static List<LevelData> LevelsData;
    
    private static List<LevelEditor> LevelEditors;

    private static OdinMenuTree tree;
    
    [MenuItem("Editor Tools/Level Editor")]
    private static void OpenWindow()
    {
        GetWindow<LevelsEditor>().Show();
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
            Database.LevelsConfiguration.LevelsData.Add(new LevelData());
            EditorUtility.SetDirty(Database.LevelsConfiguration);
        }
        
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("- Remove Level",  GUILayout.Width(100)) && Database.LevelsConfiguration.LevelsData.Count == tree.MenuItems.Count)
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
                Database.LevelsConfiguration.LevelsData.RemoveAt(levelIndex);
            }
            EditorUtility.SetDirty(Database.LevelsConfiguration);
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
        LevelEditors = new List<LevelEditor>();
        tree.Add("Game Configs", Database.GameConfiguration, EditorIcons.SettingsCog);
        for (int i = 0; i < Database.LevelsConfiguration.LevelsData.Count; i++)
        {
            LevelEditor levelEditor = new LevelEditor(i);
            LevelEditors.Add(levelEditor);
            tree.Add("Level " + (i+1), levelEditor);
        }
        
        return tree;
    }
}
