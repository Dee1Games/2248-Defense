using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class EnemyData2
{
    public int N;
    [GUIColor("GetTypeColorGUI")]
    public EnemyType Type;
    public int Health;
    public Vector2 X = new Vector2(0, 4);
    public Vector2 Y = new Vector2(0, 3);
    public float T;
        
    private UnityEngine.Color GetTypeColorGUI()
    {
        UnityEngine.Color color;
        if (Type == EnemyType.SimpleEnemy)
        {
            color = new UnityEngine.Color(0.2f, 1f, 0.2f);
        }
        else
        {
            color = new UnityEngine.Color(1f, 0.2f, 0.2f);
        }
        return color;
    }
}