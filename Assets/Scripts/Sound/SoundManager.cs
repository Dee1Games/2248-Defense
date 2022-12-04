using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private static SoundManager _instance;

    public List<SoundFile> Sounds;
    public List<Sound> ExcludedSounds;

    void Awake()
    {
        if(_instance == null)
            _instance = this;
    }

    public void Play(Sound sound , float volume = 1)
    {
        if (IsExcluded(sound))
            return;
        
        AudioSource.PlayClipAtPoint(SoundUtils.GetClip(sound), Vector3.zero, volume);
    }

    public void PlayByASComponent(Sound sound,GameObject obj, float volume = 1, float pitch = 1, bool loop = false)
    {
        AudioSource source;
        obj.TryGetComponent(out source);

        if(source == null) 
        { 
            source = obj.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;
        source.clip = SoundUtils.GetClip(sound);
        source.Play();
    }

    private bool IsExcluded(Sound sound)
    {
        return ExcludedSounds.Contains(sound);
    }
}
