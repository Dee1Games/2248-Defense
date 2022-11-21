using UnityEngine;
using TMPro;

public enum UIState
{
    MainMenu,
    InGame,
    Victory,
    Defeat
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private static UIManager _instance;

    [Header("MainMenu")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] TMP_Text mainMenuLevelText, mainMenuCashText;
    
    [Header("In Game")]
    [SerializeField] GameObject inGamePanel;
    [SerializeField] GameObject checkIcon;
    [SerializeField] TMP_Text inGameLevelText, inGameCashText;
    [SerializeField] private Animator cashAnimator;
    
    [Header("Victory")]
    [SerializeField] GameObject victoryPanel;
    [SerializeField] TMP_Text victoryLevelText, victoryPeopleNumberText;
    
    [Header("Defeat")]
    [SerializeField] GameObject defeatPanel;
    [SerializeField] TMP_Text defeatLevelText;
    [SerializeField] TMP_Text zombiesKilledText;

    [Header("Mute")] 
    [SerializeField] private GameObject muteGO;
    [SerializeField] private GameObject unmuteGO;
    [SerializeField] private GameObject vibrateOnGO;
    [SerializeField] private GameObject vibrateOffGO;
    [SerializeField] private AudioSource[] audioSources;

    public UIState State
    {
        set
        {
            bool refresh = (_state == value);
            
            mainMenuPanel.SetActive(false);
            inGamePanel.SetActive(false);
            victoryPanel.SetActive(false);
            defeatPanel.SetActive(false);
            
            _state = value;
            switch (_state)
            {
                case UIState.MainMenu:
                    mainMenuLevelText.text = "Level " + (PlayerPrefsManager.Level+1);
                    mainMenuCashText.text = PlayerPrefsManager.Coin.ToString();
                    mainMenuPanel.SetActive(true);
                    break;
                case UIState.InGame:
                    inGameLevelText.text = "Level " + (GameManager.Instance.CurrentLevelIndex + 1);// + PlayerPrefsManager.FinishedLevelsCounter * 21);
                    inGameCashText.text = GameManager.Instance.CurrentKills.ToString();
                    inGamePanel.SetActive(true);
                    break;
                case UIState.Victory:
                    victoryLevelText.text = "Level " + (GameManager.Instance.CurrentLevelIndex + 1);
                    victoryPeopleNumberText.text = "x" + GameManager.Instance.CurrentKills.ToString();
                    victoryPanel.SetActive(true);
                    PlayerPrefsManager.Coin += GameManager.Instance.CurrentKills;
                    //SoundManager.Instance.PlayCheerSound();
                    //SoundManager.Instance.PlaySuccessSound();
                    break;
                case UIState.Defeat:
					//GameManager.Instance.CanDraw = false;
                    defeatLevelText.text = "Level " + (GameManager.Instance.CurrentLevelIndex + 1);
                    zombiesKilledText.text = "x" + GameManager.Instance.CurrentKills;
                    defeatPanel.SetActive(true);
                    break;
            }
        }
        get
        {
            return _state;
        }
    }

    private UIState _state;
    
    private void Awake()
    {
        if (_instance==null)
        {
            _instance = this;
        }
    }

    //void Start()
    //{
    //    if(PlayerPrefsManager.Mute)
    //        Mute();
    //    else
    //        Unmute();
        
    //    if(PlayerPrefsManager.Vibrate)
    //        VibrateOn();
    //    else
    //        VibrateOff();
    //}

    public void Refresh()
    {
        State = _state;
    }

    //public void Mute()
    //{
    //    unmuteGO.SetActive(true);
    //    muteGO.SetActive(false);
    //    SoundManager.Instance.Mute();
    //    BusManager.Instance.MuteBusSound();
    //    PlatformCreator.Instance.MuteActrivePassengers();
    //}

    //public void Unmute()
    //{
    //    unmuteGO.SetActive(false);
    //    muteGO.SetActive(true);
    //    SoundManager.Instance.UnMute();
    //    BusManager.Instance.UnmuteBusSound();
    //    PlatformCreator.Instance.UnmuteActrivePassengers();
    //}
    
    //public void VibrateOn()
    //{
    //    vibrateOnGO.SetActive(true);
    //    vibrateOffGO.SetActive(false);
    //    PlayerPrefsManager.Vibrate = true;
    //}

    //public void VibrateOff()
    //{
    //    vibrateOnGO.SetActive(false);
    //    vibrateOffGO.SetActive(true);
    //    PlayerPrefsManager.Vibrate = false;
    //}

    public void UpdateKillCount()
    {
        cashAnimator.SetTrigger("GetMoney");
        inGameCashText.text = GameManager.Instance.CurrentKills.ToString();
    }

    public void OnClick_MainMenu_Play()
    {
        GameManager.Instance.StartCurrentLevel();
    }
    
    public void OnClick_Win_Next()
    {
        GameManager.Instance.StartCurrentLevel();
    }
    
    public void OnClick_Defeat_Retry()
    {
        GameManager.Instance.StartCurrentLevel();
    }
    
    public void OnClick_InGame_Retry()
    {
        GameManager.Instance.StartCurrentLevel();
    }
}

