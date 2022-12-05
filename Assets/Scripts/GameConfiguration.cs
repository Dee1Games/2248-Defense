using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameConfiguration), menuName = "ScriptableObjects/" + nameof(GameConfiguration))]
public class GameConfiguration : ScriptableObject
{
    [Header("Soldier")]
    public float SoldierSpeedNormal = 1f;
    public float SoldierSpeedBomber = 1f;
    public float InsideShootingRadius = Mathf.Sqrt(2);
    public float SoldiersFireRate = 0.3f;
    public float SoldiersMergeTime = 0.1f;
    public float ConnectPitchDifference = 0.2f;

    [Header("Bomber")]
    public float BomberExplosionRadius = 1.5f;
    public float BomberExplosionDamage = 100f;
    public float MinDistanceToExplode = 0.2f;
    public Color BomberColor;

    [Header("Enemy")]
    public float EnemySpeedSimple = 0.5f;
    public float EnemySpeedGiant= 0.3f;
    public float EnemyDefaultAnimationSpeed = 0.2f;
    public float EnemyAttackRate = 1f;

    [Header("Bullet")]
    public float BulletDestructionTime = 6f;
    public float BulletSpeed = 3f;
    public float BulletOffsetFromGround = 0.7f;
    public bool WallsStopBullet = true; 

    [Header("UI")]
    public float LineOffsetFromGround = 0.1f;
    public List<Color> NumberColors;
    public float WinLoseDialogeDelay = 1;

    [Header("Level")]
    public float levelsCoefficient = 0.1f;
}
