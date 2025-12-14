using UnityEngine;
using UnityEngine.Audio;

public class GameManager {
	static GameManager pInstance = null;
	
	GameStats stats;
	public EventManager eventManager;

	public static GameManager Instance() {
		if (pInstance == null) {
			pInstance = new GameManager();
			pInstance.Initialize();
		}
		return pInstance;
	}
	
	void Initialize() {
		SaveData saveData = SaveSystem.LoadSave("save");

		if (saveData != null) {
			stats = saveData.AsStats();
		} else {
			stats = new GameStats();	
		}
		
		Resources.Load<AudioMixerGroup>("Music").audioMixer.SetFloat("Volume", stats.musicVol);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.SetFloat("Volume", stats.sfxVol);

		eventManager = new EventManager();

		AlexLang.ParseFile();
	}
	
	public void SaveGame() {
		Resources.Load<AudioMixerGroup>("Music").audioMixer.GetFloat("Volume", out stats.musicVol);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.GetFloat("Volume", out stats.sfxVol);

		SaveSystem.Save(stats);
	}

	/// <summary>
	/// Runs the moment a new scene is loaded. Put anything that should happen
	/// right after a new scene is loaded here.
	/// </summary>
	public void Bootstrap() {
		Resources.Load<AudioMixerGroup>("Music").audioMixer.SetFloat("Volume", stats.musicVol);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.SetFloat("Volume", stats.sfxVol);
	}

	/// <summary>
	/// Runs the moment before a new scene is loaded. Put anything that should happen right before
	/// a new scene is loaded here.
	/// </summary>
	public void Teardown() {
		eventManager.ClearRegistry();

		if(InputManager.instance != null) {
			InputManager.instance.ClearHandlers();
		}

		LeanTween.cancelAll();

		Resources.Load<AudioMixerGroup>("Music").audioMixer.GetFloat("Volume", out stats.musicVol);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.GetFloat("Volume", out stats.sfxVol);
	}
}