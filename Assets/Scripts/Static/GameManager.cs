using UnityEngine;
using UnityEngine.Audio;

public class GameManager {
	static GameManager pInstance = null;
	
	GameStats stats;
	public EventManager eventManager;

	static readonly float timeScale = 1F;
	float timeOffset;
	float lastPausedAtTime;
	
	bool paused;

	static float RealtimeSinceLevelLoad() {
		return Time.timeSinceLevelLoad;
	}
	
	public float TimeSinceLevelLoad() {
		if (paused) {
			return lastPausedAtTime;
		}

		return RealtimeSinceLevelLoad() - timeOffset;
	}
	
	public void SetPaused(bool paused) {
		this.paused = paused;
		UpdatePause();
	}

	public void TogglePaused() {
		SetPaused(!paused); 
	}
	
	void UpdatePause() {
		if (paused) {
			lastPausedAtTime = RealtimeSinceLevelLoad();
		} else {
			timeOffset += RealtimeSinceLevelLoad() - lastPausedAtTime;
		}
	}
	
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
			stats = saveData.AsGameStats();
		} else {
			stats = new GameStats();
		}
		
		Resources.Load<AudioMixerGroup>("Music").audioMixer.SetFloat("Volume", stats.musicVolume);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.SetFloat("Volume", stats.sfxVolume);

		eventManager = new EventManager();

		AlexLang.ParseFile();
	}
	
	/// <summary>
	/// Saves the game.
	/// </summary>
	public void SaveGame() {
		Resources.Load<AudioMixerGroup>("Music").audioMixer.GetFloat("Volume", out stats.musicVolume);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.GetFloat("Volume", out stats.sfxVolume);

		SaveSystem.Save(stats);
	}

	/// <summary>
	/// Runs the moment a new scene is loaded. Put anything that should happen
	/// right after a new scene is loaded here.
	/// </summary>
	public void Bootstrap() {
		Resources.Load<AudioMixerGroup>("Music").audioMixer.SetFloat("Volume", stats.musicVolume);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.SetFloat("Volume", stats.sfxVolume);
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

		Resources.Load<AudioMixerGroup>("Music").audioMixer.GetFloat("Volume", out stats.musicVolume);
		Resources.Load<AudioMixerGroup>("SFX").audioMixer.GetFloat("Volume", out stats.sfxVolume);
	}
}