using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    [Range(0, 10)]
    public int MergeCount = 4;
    
    [Range(1, 8)]
    public int Map;

    public MapTheme Theme;
    public int MinSoldierPower = 1;
    public int MaxSoldierPower = 6;
    public float BomberSoldierSpawnProbability = 0f;
    public float WaitTimeBeforeSpawnLevel;
    [ListDrawerSettings(ShowPaging = true, NumberOfItemsPerPage = 1, Expanded = true)]
    public List<WaveData> WavesData;
    
    public int[] MatrixRow1 = new int[5];
    public int[] MatrixRow2 = new int[5];
    public int[] MatrixRow3 = new int[5];
    public int[] MatrixRow4 = new int[5];

    public bool HasTutorial= false;
    public List<TutorialPath> Tutorials;

    public void SetMatrix(int[,] matrix)
    {
        MatrixRow1 = new int[5];
        MatrixRow2 = new int[5];
        MatrixRow3 = new int[5];
        MatrixRow4 = new int[5];

        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                if (y == 0)
                {
                    MatrixRow1[x] = matrix[x, y];
                }
                else if (y == 1)
                {
                    MatrixRow2[x] = matrix[x, y];
                }
                else if (y == 2)
                {
                    MatrixRow3[x] = matrix[x, y];
                }
                else
                {
                    MatrixRow4[x] = matrix[x, y];
                }
            }
        }
    }
    
    public int[,] GetMatrix()
    {
        int[,] matrix = new int[5,4];

        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                if (y == 0)
                {
                    matrix[x, y] = (MatrixRow1.Length==5)?MatrixRow1[x]:0;
                }
                else if (y == 1)
                {
                    matrix[x, y] = (MatrixRow2.Length==5)?MatrixRow2[x]:0;
                }
                else if (y == 2)
                {
                    matrix[x, y] = (MatrixRow3.Length==5)?MatrixRow3[x]:0;
                }
                else
                {
                    matrix[x, y] = (MatrixRow4.Length==5)?MatrixRow4[x]:0;
                }
            }
        }

        return matrix;
    }

    public int GetCellNumber(int x, int y)
    {
        if (y == 0)
        {
            return MatrixRow1[x];
        }
        else if (y == 1)
        {
            return MatrixRow2[x];
        }
        else if (y == 2)
        {
            return MatrixRow3[x];
        }
        else
        {
            return MatrixRow4[x];
        }
    }
}
