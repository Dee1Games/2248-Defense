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

    public static int Coin
    {
        set
        {
            PlayerPrefs.SetInt(CoinSaveAddress, value);
        }
        get
        {
            int result = PlayerPrefs.GetInt(CoinSaveAddress, 0);
            return result;
        }
    }

    public static bool SeenTutorial
    {
        set
        {
            PlayerPrefs.SetInt(TutorialSaveAddress, value?1:0);
        }
        get
        {
            int result = PlayerPrefs.GetInt(TutorialSaveAddress, 0);
            return result==1;
        }
    }
    
    private const string LevelSaveAddress = "Level";
    private const string CoinSaveAddress = "Coin";
    private const string TutorialSaveAddress = "Tutorial";
}