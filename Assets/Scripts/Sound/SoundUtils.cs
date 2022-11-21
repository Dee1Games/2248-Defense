using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundUtils
{
    private static Dictionary<Sound, AudioClip> sounds;

    public static AudioClip GetClip(Sound sound)
    {
        if (sounds == null || sounds.Count == 0)
        {
            sounds = new Dictionary<Sound, AudioClip>();

            foreach (var soundFile in SoundManager.Instance.Sounds)
            {
                if(!sounds.ContainsKey(soundFile.Sound))
                    sounds.Add(soundFile.Sound, soundFile.Clip);
            }
        }

        return sounds[sound];
    }
}
