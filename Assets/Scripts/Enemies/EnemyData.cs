using Sirenix.OdinInspector;

[System.Serializable]
public class EnemyData
{
    [GUIColor("GetTypeColorGUI")]
    public EnemyType Type;
    public float Health = 100;
    public float X;
    public float YOffset;
    public float SpawnTime;

    public void Copy(EnemyData data)
    {
        Type = data.Type;
        Health = data.Health;
        X = data.X;
        YOffset = data.YOffset;
    }
    
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
