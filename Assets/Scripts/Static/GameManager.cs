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
			//Debug.Log("GameManager Awake");
			// The very first time someone calls Instance() we populate pInstance
			pInstance = new GameManager();
			// and call its InitGame
			pInstance.InitGame();
		}
		return pInstance;
	}

	public void SaveGame() {
		Resources.Load<AudioMixerGroup>("Music").audioMixer.GetFloat("Volume", out stats.musicVol);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.GetFloat("Volume", out stats.sfxVol);

		SaveSystem.SavePlayer(stats);
	}

	// Note to dad: this is called once and only once in the beginning of the game
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