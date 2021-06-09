using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
            for (int i = 0; i < s.amountOfSources; i++)
            {
                s.sources.Add(gameObject.AddComponent<AudioSource>());
            }

            foreach (var source in s.sources)
            {
                source.clip = s.clips[0];

                source.volume = s.volume;
                source.pitch = s.pitch;

                source.loop = s.loop;
                source.outputAudioMixerGroup = s.mixer;
            }
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return;
        }

        if (s.clips.Count > 1)
        {
            s.sources[s.currentlyPlayingFrom].clip = s.clips[UnityEngine.Random.Range(0, s.clips.Count)];
        }

        s.sources[s.currentlyPlayingFrom].pitch = s.pitch;
        s.sources[s.currentlyPlayingFrom].Play();
        s.currentlyPlayingFrom = IncrementWithOverflow.Run(s.currentlyPlayingFrom, s.amountOfSources, 1);
    }

    public void MasterPlay(string name, float minPitch = 1F, float maxPitch = 1F, bool onlyIfDone = false)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return;
        }

        bool abort = false;
        
        foreach (var audioSource in s.sources)
        {
            if (audioSource.isPlaying)
            {
                abort = true;
            }
        }

        if (onlyIfDone && abort)
        {
            return;
        }

        if (s.clips.Count > 1)
        {
            s.sources[s.currentlyPlayingFrom].clip = s.clips[UnityEngine.Random.Range(0, s.clips.Count)];
        }

        s.sources[s.currentlyPlayingFrom].pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        s.sources[s.currentlyPlayingFrom].Play();
        s.currentlyPlayingFrom = IncrementWithOverflow.Run(s.currentlyPlayingFrom, s.amountOfSources, 1);
    }

    public void PlayOnlyIfDone(string name, float minPitch = 1, float maxPitch = 1)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        
        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return;
        }

        bool abort = false;

        foreach (var audioSource in s.sources)
        {
            if(audioSource.isPlaying)
            {
                abort = true;
            }
        }

        if(abort)
        {
            return;
        }

        if (s.clips.Count > 1)
        {
            s.sources[s.currentlyPlayingFrom].clip = s.clips[UnityEngine.Random.Range(0, s.clips.Count)];
        }

        s.sources[s.currentlyPlayingFrom].pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        s.sources[s.currentlyPlayingFrom].Play();
        s.currentlyPlayingFrom = IncrementWithOverflow.Run(s.currentlyPlayingFrom, s.amountOfSources, 1);
    }

    public void Play(string name, float minPitch, float maxPitch)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return;
        }

        if (s.clips.Count > 1)
        {
            s.sources[s.currentlyPlayingFrom].clip = s.clips[UnityEngine.Random.Range(0, s.clips.Count)];
        }

        s.sources[s.currentlyPlayingFrom].pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        s.sources[s.currentlyPlayingFrom].Play();
        s.currentlyPlayingFrom = IncrementWithOverflow.Run(s.currentlyPlayingFrom, s.amountOfSources, 1);
    }

    public void StopAllSources(string name, bool resetTrack)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return;
        }

        foreach (var source in s.sources)
        {
            source.Stop();
        }

        if (resetTrack)
        {
            foreach (var source in s.sources)
            {
                source.time = 0;
            }
        }
    }

    public void StopSingleSource(string name, bool resetTrack, int index)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return;
        }

        s.sources[index].Stop();

        if (resetTrack)
        {
            s.sources[index].time = 0;
        }
    }

    public void MuteAllSources(string name, bool mute)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return;
        }

        foreach (var source in s.sources)
        {
            source.volume = mute ? 0 : s.volume;
        }
    }

    public void MuteSingleSource(string name, bool mute, int index)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return;
        }

        s.sources[index].volume = mute ? 0 : s.volume;
    }

    public bool AnySourceIsPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return false;
        }

        foreach (var source in s.sources)
        {
            if (source.isPlaying)
                return true;
            else
                return false;
        }

        return false;
    }

    public bool SingleSourceIsPlaying(string name, int index)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Sophocles: '{name}' nao existe. Tente outro nome, por favor.");
            return false;
        }

        return s.sources[index].isPlaying;
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (lastSong != null && s != null)
        {
            if (lastSong.clip.name == s.sources[0].clip.name)
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
                yield return new WaitForSecondsRealtime(0.05F);
            }

            lastSong.Stop();
            lastSong.time = 0;
            lastSong.volume = 1;
        }

        lastSong = s.sources[0];

        lastSong.volume = 0;
        lastSong.Play();

        while (lastSong.volume < targetMusicVolume)
        {
            lastSong.volume += 0.1F;
            yield return new WaitForSecondsRealtime(0.05F);
        }
    }

    public float targetMusicVolume = 1F;
}