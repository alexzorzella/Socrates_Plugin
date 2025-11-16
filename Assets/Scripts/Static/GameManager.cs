using UnityEngine;
using UnityEngine.Audio;

public class GameManager {
	static GameManager pInstance = null;
	
	public GameStats stats;
	public EventManager eventManager;

	// Put anything that should happen right after a new scene is loaded here
	void PreSceneLoaded() {
		eventManager.ClearRegistry();

		if(InputManager.instance != null) {
			InputManager.instance.ClearHandlers();
		}

		LeanTween.cancelAll();

		Resources.Load<AudioMixerGroup>("Music").audioMixer.GetFloat("Volume", out stats.musicVol);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.GetFloat("Volume", out stats.sfxVol);
	}

	// Put anything that should happen before a new scene is loaded here
	void PostSceneLoaded() {
		Resources.Load<AudioMixerGroup>("Music").audioMixer.SetFloat("Volume", stats.musicVol);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.SetFloat("Volume", stats.sfxVol);
	}

	public static GameManager Instance() {
		if (pInstance == null) {
			pInstance = new GameManager();
			pInstance.InitGame();
		}
		return pInstance;
	}

	public void SaveGame() {
		Resources.Load<AudioMixerGroup>("Music").audioMixer.GetFloat("Volume", out stats.musicVol);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.GetFloat("Volume", out stats.sfxVol);

		SaveSystem.SavePlayer(stats);
	}
	
	void InitGame() {
		stats = new GameStats();
		
		if (SaveSystem.LoadPlayer("save") != null) {
			Resources.Load<AudioMixerGroup>("Music").audioMixer.SetFloat("Volume", stats.musicVol);
			Resources.Load<AudioMixerGroup>("SFX").audioMixer.SetFloat("Volume", stats.sfxVol);
		}

		eventManager = new EventManager();

		AlexLang.ParseFile();
	}

	public void Bootstrap() {
		PreSceneLoaded();
	}

	public void Teardown() {
		PostSceneLoaded();
	}
}