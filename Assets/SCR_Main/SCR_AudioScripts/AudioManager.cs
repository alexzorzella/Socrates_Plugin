using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    private static AudioManager _i;

    public static AudioManager i
    {
        get
        {
            if (_i == null)
            {
                AudioManager x = Resources.Load<AudioManager>("AudioManager");

                _i = Instantiate(x);
            }
            return _i;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.mixer;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Play();
    }

    public void Stop(string name, bool resetTrack)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.Stop();

        if(resetTrack)
        {
            s.source.time = 0;
        }
    }

    public void Mute(string name, bool mute)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        
        if(mute)
        {
            s.volume = 0;
        } else if(!mute)
        {
            s.volume = 1;
        }
    }

    public bool IsPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s.source.isPlaying)
            return true;
        else
            return false;
    }

    public void RandomizeSound(string name, float pitch)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.pitch = pitch;
    }

    public AudioClip GetClip(string name)
    {
        return Array.Find(sounds, sound => sound.name == name).clip;
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (lastSong != null && s != null)
        {
            if (lastSong.clip.name == s.source.clip.name)
            {
                return;
            }
        }

        StartCoroutine(SmoothFade(s));
    }

    private AudioSource lastSong;

    private IEnumerator SmoothFade(Sound s)
    {
        if (lastSong != null)
        {
            while (lastSong.volume > 0)
            {
                lastSong.volume -= 0.1F;
                yield return new WaitForSeconds(0.05F);
            }

            lastSong.Stop();
            lastSong.time = 0;
            lastSong.volume = 1;
        }

        lastSong = s.source;

        lastSong.volume = 0;
        lastSong.Play();

        while (lastSong.volume < targetMusicVolume)
        {
            lastSong.volume += 0.1F;
            yield return new WaitForSeconds(0.05F);
        }
    }

    public float targetMusicVolume = 1F;
}