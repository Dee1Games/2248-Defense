using System;
using System.Collections;
using System.Collections.Generic;
using SupersonicWisdomSDK;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{


    private void Awake()
    {
        // Subscribe
        SupersonicWisdom.Api.AddOnReadyListener(OnSupersonicWisdomReady);
        // Then initialize
        SupersonicWisdom.Api.Initialize();
    }
    
    void OnSupersonicWisdomReady()
    {
        // Start your game from this point
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

}
