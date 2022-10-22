using UnityEngine;

public class PlayerPrefsManager
{
    public static int Level
    {
        set
        {
            PlayerPrefs.SetInt(LevelSaveAddress, value);
        }
        get
        {
            int result = PlayerPrefs.GetInt(LevelSaveAddress, 0);
            return result;
        }
    }
    
    private const string LevelSaveAddress = "Level";
}