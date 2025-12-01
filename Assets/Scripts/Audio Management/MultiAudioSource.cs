using System;
using UnityEngine;
using UnityEngine.Audio;

/**
 * Utility class that can be used to play audio clips.
 *
 * This supports mutiple sound clips to be played either in a
 * round-robin or random order. 
 */
// TODO: support changing volume (and pitch etc)

public class MultiAudioSource {

	readonly System.Random random = new System.Random();

	readonly AudioSource[] sources;

	int next;

	MultiAudioSource(params AudioSource[] sources) {
		this.sources = sources;
		next = random.Next(0, sources.Length);
	}

	public void PlayRandom() {
		this.sources[next].Play();
		this.next = random.Next(0, sources.Length);
	}

	public void PlayRandomPitch() {
		SetRandomPitch();
		PlayRandom();
	}
	
	public void SetRandomPitch() {
		sources[next].pitch = UnityEngine.Random.Range(0.8F, 1.2F);
	}

	public void PlayRoundRobin() {
		this.sources[next].Play();
		this.next = (this.next + 1) % sources.Length;
	}
	
	public void PlayIfDone() {
		foreach (var source in sources) {
			if (source.isPlaying) {
				return;
			}
		}
		
		PlayRoundRobin();
	}

	public void SetVolume(float volume) {
		foreach (var source in sources) {
			source.volume = volume;
		}
	}

	public void Stop() {
		foreach (var source in sources) {
			source.Stop();
		}
	}

	public void SetPitch(float pitch) {
		foreach (var source in sources) {
			source.pitch = pitch;
		}	
	}

	// TODO: breaks with multiple
	public string GetName() {
		return sources[0].clip.name;
	}

	/**
	 * Load a single audio clip from the given `path`. This is a relative
	 * path to any directory named `Resources` in the `Assets` directory
	 * tree.
	 *
	 * The `AudioSource` component is added to the given `gameObject`.
	 *
	 * Typically, the `gameObject` will be the `this.gameObject` from the
	 * caller of this method.
	 *
	 * This method throws if the resource does not exist.
	 */
	public static MultiAudioSource FromResource(
	  GameObject gameObject, String path, bool loop = false, string audioMixer = "SFX") {
		AudioClip clip = Resources.Load<AudioClip>(path);
		if (clip == null) {
			throw new ArgumentException("Resource not found: " + path);
		}

		var audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = clip;

		audioSource.playOnAwake = false;
		audioSource.loop = loop;
		audioSource.outputAudioMixerGroup = Resources.Load<AudioMixerGroup>(audioMixer);

		return new MultiAudioSource(audioSource);
	}

	/**
	 * This is like the `FromResource`, but for loading multiple audio
	 * clips (say to be played randomly or in a round-robbin fashion).
	 *
	 * The `count` is how many clips to load. The expected file names are
	 * `pathPrefix0`, `pathPrefix1`, ... `pathPrefix{count - 1}`.
	 *
	 * This method throws if any of the resources does not exist.
	 */
	public static MultiAudioSource FromResources(
	  GameObject gameObject, String pathPrefix, int count, string audioMixer = "SFX") {
		AudioSource[] audioSources = new AudioSource[count];

		for (int i = 0; i < count; i++) {
			AudioClip clip = Resources.Load<AudioClip>($"{pathPrefix}_{i}");
			if (clip == null) {
				throw new ArgumentException("Resource not found: " + pathPrefix + i);
			}
			audioSources[i] = gameObject.AddComponent<AudioSource>();
			audioSources[i].clip = clip;
			audioSources[i].outputAudioMixerGroup = Resources.Load<AudioMixerGroup>(audioMixer);
		}

		return new MultiAudioSource(audioSources);
	}
}