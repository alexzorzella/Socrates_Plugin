using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroFunc : MonoBehaviour {
	DeltaCameraShake cameraShake;

	public Transform[] letters;

	public int letterFlipIndex = -1;
	public float lerpSpeed = 5F;
	public float lerpTime = 0;

	readonly Dictionary<string, MultiAudioSource> audioSources = new Dictionary<string, MultiAudioSource>();

	void PlayAudioSource(string soundName) {
		if (!audioSources.ContainsKey(soundName)) {
			audioSources.Add(soundName, MultiAudioSource.FromResource(gameObject, soundName));
		}

		audioSources[soundName].PlayRandom();
	}

	void StopAudioSource(string soundName) {
		if (!audioSources.ContainsKey(soundName)) {
			return;
		}

		audioSources[soundName].Stop();
	}

	public void ResetLetters() {
		letterFlipIndex = -1;

		foreach (var letter in letters) {
			letter.transform.rotation = Quaternion.Euler(new Vector3(0, -90F, 0));
		}
	}

	public void EngageLetterScroll() {
		letterFlipIndex = 0;
		lerpTime = 0;
	}

	public void UpdateLetters() {
		if (letterFlipIndex != -1 && letterFlipIndex < letters.Length) {
			if (letters[letterFlipIndex].transform.rotation.eulerAngles.y == 0F) {
				letters[letterFlipIndex].transform.rotation = Quaternion.identity;
				letterFlipIndex++;
				lerpTime = 0;

				if (letterFlipIndex >= letters.Length)
					return;
			}

			letters[letterFlipIndex].rotation = Quaternion.Lerp(letters[letterFlipIndex].rotation, Quaternion.identity, lerpTime);
			lerpTime += Time.deltaTime * lerpSpeed;
		}
	}

	private void Start() {
		cameraShake = Camera.main.GetComponent<DeltaCameraShake>();
		ResetLetters();
	}

	public void Shake() {
		cameraShake.Shake(Shakepedia.GetProfileClone(Shakepedia.MEDIUM_RARE));
	}

	public void ShakeLight() {
		cameraShake.Shake(Shakepedia.GetProfileClone(Shakepedia.RUMBLE));
	}

	public TextMeshProUGUI animationNameDisplay;
	public Animator animator;

	public void Update() {
		UpdateLetters();
	}

	public void PlaySound(string sound) {
		string[] sounds = sound.Split(',');

		foreach (var s in sounds) {
			PlayAudioSource(s);
			// AudioManager.i.Play(s);
		}
	}

	public void StopSound(string sound) {
		StopAudioSource(sound);
		// AudioManager.i.StopAllSources(sound, true);
	}

	public GameObject guts;
	public Transform guts_position;

	public void Guts() {
		Instantiate(guts, guts_position.position, Quaternion.identity);
	}

	public void LoadScene(string sceneName) {
		NATransition.i.LoadScene(sceneName);
	}
}