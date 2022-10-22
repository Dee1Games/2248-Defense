using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameConfiguration), menuName = "ScriptableObjects/" + nameof(GameConfiguration))]
public class GameConfiguration : ScriptableObject
{
    public float SoldiersFireRate = 0.3f;
    public float BulletDestructionTime = 6f;
    public float BulletOffsetFromGround = 0.7f;
    public float LineOffsetFromGround = 0.1f;
    public List<Color> NumberColors;
}
