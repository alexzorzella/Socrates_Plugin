using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Utility class that can be used to play audio clips.
///
/// This supports multiple sound clips to be played either in a
/// round-robin or random order. Clip pitches can be set and randomized.
/// Clip volumes can be set.
///
/// Clips are automatically loaded from any Resources folder and are either
/// expected to be named `clip_name` in the case of audio sources with one
/// clip and `clip_name_0`...`clip_name_{count - 1}` in the case of audio
/// sources with multiple audio sources.
/// </summary>
public class MultiAudioSource {
	readonly System.Random random = new();
	readonly AudioSource[] sources;
	int next;

	/// <summary>
	/// Instances of MultiAudioSource can only be constructed via the `FromResource` and
	/// `FromResources` functions.
	/// </summary>
	/// <param name="sources"></param>
	MultiAudioSource(params AudioSource[] sources) {
		this.sources = sources;
		next = random.Next(0, sources.Length);
	}

	/// <summary>
	/// Returns the name of the clip loaded into the first audio source.
	/// </summary>
	/// <returns></returns>
	public string GetName() {
		return sources[0].clip.name;
	}
	
	/// <summary>
	/// Sets the volume of all the sources to the passed volume.
	/// </summary>
	/// <param name="volume"></param>
	public void SetVolume(float volume) {
		foreach (var source in sources) {
			source.volume = volume;
		}
	}
	
	/// <summary>
	/// Sets the pitch of all the audio sources to the passed pitch.
	/// </summary>
	/// <param name="pitch"></param>
	public void SetPitch(float pitch) {
		foreach (var source in sources) {
			source.pitch = pitch;
		}	
	}
	
	/// <summary>
	/// Randomly sets the pitch of all the audio sources.
	/// </summary>
	public void RandomizePitches(float minPitch = 0.8F, float maxPitch = 1.2F) {
		if (maxPitch < minPitch) {
			maxPitch = minPitch;
		}
		
		foreach (var source in sources) {
			source.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
		}
	}

	/// <summary>
	/// An alias for PlayRoundRobin()
	/// </summary>
	public void Play() {
		PlayRoundRobin();
	}
	
	/// <summary>
	/// Plays the next audio clip in the rotation of loaded clips.
	/// </summary>
	public void PlayRoundRobin() {
		sources[next].Play();
		next = (next + 1) % sources.Length;
	}
	
	/// <summary>
	/// Plays a random audio clip from the rotation loaded clips.
	/// </summary>
	public void PlayRandom() {
		sources[next].Play();
		next = random.Next(0, sources.Length);
	}
	
	/// <summary>
	/// Plays a random audio clip from the rotation of loaded clips
	/// if none of the sources are currently playing.
	/// </summary>
	public void PlayOnlyIfDone() {
		foreach (var source in sources) {
			if (source.isPlaying) {
				return;
			}
		}
		
		PlayRandom();
	}

	/// <summary>
	/// Stops all the audio sources.
	/// </summary>
	public void Stop() {
		foreach (var source in sources) {
			source.Stop();
		}
	}

	/// <summary>
	/// Load a single audio clip from the given `path`. This is a relative
	/// path to any directory named `Resources` in the `Assets` directory
	/// tree.
	/// 
	/// The `AudioSource` component is added to the given `gameObject`.
	/// 
	/// Typically, the `gameObject` will be the `this.gameObject` from the
	/// caller of this method.
	/// 
	/// This method throws if the resource does not exist.
	/// </summary>
	/// <param name="gameObject"></param>
	/// <param name="path"></param>
	/// <param name="loop"></param>
	/// <param name="audioMixer"></param>
	/// <param name="volume"></param>
	/// <param name="pitch"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static MultiAudioSource FromResource(
	  GameObject gameObject, string path, bool loop = false, string audioMixer = "SFX", float volume = 1F, float pitch = 1F) {
		AudioClip clip = Resources.Load<AudioClip>(path);
		if (clip == null) {
			throw new ArgumentException("Resource not found: " + path);
		}

		var audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = clip;

		audioSource.playOnAwake = false;
		audioSource.loop = loop;
		audioSource.outputAudioMixerGroup = Resources.Load<AudioMixerGroup>(audioMixer);

		audioSource.volume = volume;
		audioSource.pitch = pitch;

		return new MultiAudioSource(audioSource);
	}

	/// <summary>
	/// This is like the `FromResource`, but for loading multiple audio
	/// clips (say to be played randomly or in a round-robbin fashion).
	/// 
	/// The `count` is how many clips to load. The expected file names are
	/// `pathPrefix_0`, `pathPrefix_1`, ... `pathPrefix_{count - 1}`.
	/// 
	/// This method throws if any of the resources does not exist. 
	/// </summary>
	/// <param name="gameObject"></param>
	/// <param name="pathPrefix"></param>
	/// <param name="count"></param>
	/// <param name="audioMixer"></param>
	/// <param name="volume"></param>
	/// <param name="pitch"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	public static MultiAudioSource FromResources(
	  GameObject gameObject, string pathPrefix, int count, string audioMixer = "SFX", float volume = 1F, float pitch = 1F) {
		AudioSource[] audioSources = new AudioSource[count];

		for (int i = 0; i < count; i++) {
			AudioClip clip = Resources.Load<AudioClip>($"{pathPrefix}_{i}");
			if (clip == null) {
				throw new ArgumentException("Resource not found: " + pathPrefix + i);
			}
			audioSources[i] = gameObject.AddComponent<AudioSource>();
			audioSources[i].clip = clip;
			audioSources[i].outputAudioMixerGroup = Resources.Load<AudioMixerGroup>(audioMixer);
			
			audioSources[i].volume = volume;
			audioSources[i].pitch = pitch;
		}

		return new MultiAudioSource(audioSources);
	}
}