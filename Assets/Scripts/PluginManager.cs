using System.Collections;
using System.Collections.Generic;
//using AppsFlyerSDK;
using Facebook.Unity;
//using GameAnalyticsSDK;
using UnityEngine;

public class PluginManager : MonoBehaviour
{
    public static PluginManager Instance
    {
        get
        {
            return _instance;
        }
    }
    private static PluginManager _instance;

    // [Header("MainMenu")]
    // [SerializeField] private string AppsFlyerDevkey = "";
    // [SerializeField] private string AppsFlyerAppID = "";
    // [SerializeField] private bool AppsFlyerDebug = false;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        // AppsFlyer.setIsDebug(AppsFlyerDebug);
        // AppsFlyer.initSDK(AppsFlyerDevkey, AppsFlyerAppID);
        // AppsFlyer.startSDK();
        
        FB.Init();
        //GameAnalytics.Initialize();
    }
}