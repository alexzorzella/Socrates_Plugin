using UnityEngine.Audio;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;

    public List<AudioClip> clips;

    [Range(1, 10)]
    public int amountOfSources = 1;
    [HideInInspector]
    public int currentlyPlayingFrom = 0;

    [Range(0f, 1)]
    public float volume;
    [Range(0.1f, 3)]
    public float pitch;

    public bool loop;

    public AudioMixerGroup mixer;

    [HideInInspector]
    public List<AudioSource> sources;
}