using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinDialog : MonoBehaviour
{
    public void PlayStarSound()
    {
        SoundManager.Instance.Play(Sound.Star);
    }
}
