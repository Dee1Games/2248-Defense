using Sirenix.OdinInspector;

[System.Serializable]
public class EnemyData
{
    [GUIColor("GetTypeColorGUI")]
    public EnemyType Type;
    [GUIColor("GetEnemyColorGUI")]
    public EnemyColor Color;
    public float Health = 100;
    public float X;

    public void Copy(EnemyData data)
    {
        Type = data.Type;
        Color = data.Color;
        Health = data.Health;
        X = data.X;
    }

    public UnityEngine.Color GetColor()
    {
        switch (Color)
        {
            case EnemyColor.Blue:
                return UnityEngine.Color.blue;
            case EnemyColor.Green:
                return UnityEngine.Color.green;
            case EnemyColor.Pink:
                return new UnityEngine.Color(0.9f, 0.3f, 0.4f);
            case EnemyColor.Purple:
                return UnityEngine.Color.magenta;
            case EnemyColor.Red:
                return UnityEngine.Color.red;
            case EnemyColor.Yellow:
                return UnityEngine.Color.yellow;
            default:
                return UnityEngine.Color.grey;
        }
    }
    
    private UnityEngine.Color GetEnemyColorGUI()
    {
        UnityEngine.Color color = GetColor();
        return color;
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
