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

	public void Play(string name, float minPitch = 1F, float maxPitch = 1F)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Erro de áudio. Sophocles diz: O som '{name}' não existe.");
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

    public void SetVolume(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Erro de sóm. Sophocles diz: '{name}' não existe.");
            return;
        }

        s.sources[s.currentlyPlayingFrom].volume = volume;
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Erro de sóm. Sophocles diz: '{name}' não existe.");
            return;
        }

        s.sources[s.currentlyPlayingFrom].Pause();
    }

    public void PlayOnlyIfDone(string name, float minPitch = 1, float maxPitch = 1)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Erro de áudio. Sophocles diz: O som '{name}' não existe.");
            return;
        }

        foreach (var audioSource in s.sources)
        {
            if (audioSource.isPlaying)
            {
                return;
            }
        }

		Play(name, minPitch, maxPitch);
    }

    public void PlayMany(string name, int count)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Erro de sóm. Sophocles diz: '{name}' não existe.");
            return;
        }
        
		for(int i = 0; i < count; i++)
		{
			Play(name);
		}
    }

    public void StopAllSources(string name, bool resetPlayback)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Erro de áudio. Sophocles diz: O som '{name}' não existe.");
            return;
        }

        foreach (var source in s.sources)
        {
            source.Stop();

			if (resetPlayback)
			{
				source.time = 0;
			}
        }
    }

    public void MuteAllSources(string name, bool mute)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogError($"Erro de áudio. Sophocles diz: O som '{name}' não existe.");
            return;
        }

        foreach (var source in s.sources)
        {
            source.volume = mute ? 0 : s.volume;
        }
    }
}