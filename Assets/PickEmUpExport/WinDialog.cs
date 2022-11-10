using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinDialog : MonoBehaviour
{
    [SerializeField]  AudioSource src;

    private int starCounter = 0;

    public void PlayStarSound()
    {
        //SoundManager.Instance.PlayStarSound(1 + starCounter * 0.1f);
        starCounter = starCounter + 1 == 3 ? 0 : starCounter + 1;

    }
}
