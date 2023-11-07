using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    //Singleton sound manager to easily play a sound from anywhere
    private static SoundManager _instance;

    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("NULL SoundManager");
            return _instance;
        }
    }

    public List<Sound> SoundObjects;

    Dictionary<SoundType, Sound> Sounds = new Dictionary<SoundType, Sound>();

    private void Awake()
    {
        _instance = this;
        foreach(var sound in SoundObjects)
        {
            Sounds.Add(sound.SoundType, sound);
        }
        PlaySound(SoundType.Engine, true);
    }

    public void PlaySound(SoundType soundType, bool loop = false)
    {
        if (Sounds.TryGetValue(soundType, out Sound sound))
        {
            sound.AudioSource.loop = loop;
            sound.AudioSource.Play();
        }
    }

    public void StopSound(SoundType soundType)
    {
        if (Sounds.TryGetValue(soundType, out Sound sound))
        {
            sound.AudioSource.Stop();
        }
    }

    public void AdjustSoundPitch(SoundType soundType, float pitch)
    {
        if (Sounds.TryGetValue(soundType, out Sound sound))
        {
            sound.AudioSource.pitch = pitch;
        }
    }
}

