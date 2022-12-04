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
    
    
    public static LevelsConfiguration2 LevelsConfiguration2
    {
        get
        {
            if (levelsConfiguration2 == null)
            {
                levelsConfiguration2 = Resources.Load<LevelsConfiguration2>("Database/" + nameof(LevelsConfiguration2));
            }
            return levelsConfiguration2;
        }
    }
    private static LevelsConfiguration2 levelsConfiguration2;
    
    
    public static TutorialConfiguration TutorialConfiguration
    {
        get
        {
            if (tutorialConfiguration == null)
            {
                tutorialConfiguration = Resources.Load<TutorialConfiguration>("Database/" + nameof(TutorialConfiguration));
            }
            return tutorialConfiguration;
        }
    }
    private static TutorialConfiguration tutorialConfiguration;

    public static void Reset()
    {
        gameConfiguration = null;
        levelsConfiguration = null;
        levelsConfiguration2 = null;
        tutorialConfiguration = null;
    }
}
