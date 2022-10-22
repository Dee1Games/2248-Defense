using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database : MonoBehaviour
{
    public static GameConfiguration GameConfiguration
    {
        get
        {
            if (gameConfiguration == null)
            {
                gameConfiguration = Resources.Load<GameConfiguration>("Database/" + nameof(GameConfiguration));
            }
            return gameConfiguration;
        }
    }
    private static GameConfiguration gameConfiguration;

    public static LevelsConfiguration LevelsConfiguration
    {
        get
        {
            if (levelsConfiguration == null)
            {
                levelsConfiguration = Resources.Load<LevelsConfiguration>("Database/" + nameof(LevelsConfiguration));
            }
            return levelsConfiguration;
        }
    }
    private static LevelsConfiguration levelsConfiguration;

    public static void Reset()
    {
        gameConfiguration = null;
        levelsConfiguration = null;
    }
}
