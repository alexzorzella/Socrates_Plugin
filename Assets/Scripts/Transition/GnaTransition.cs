using UnityEngine;
using UnityEngine.SceneManagement;

public class GnaTransition : MonoBehaviour {
	const float transitionAnimationLength = 0.5F;

	CanvasGroup group;

	static GnaTransition _i;

	public static GnaTransition i {
		get {
			if (_i == null) {
				GnaTransition x = Resources.Load<GnaTransition>("Transition");
				_i = Instantiate(x);
			}

			return _i;
		}
	}

	public static bool Transitioning;
	
	void Awake() {
		Initialize();
	}

	/// <summary>
	/// Initializes to not be destroyed on load, caches the
	/// canvas group, and sets its alpha to zero
	/// </summary>
	void Initialize() {
		DontDestroyOnLoad(gameObject);
		group = GetComponent<CanvasGroup>();
		group.alpha = 0;
	}

	/// <summary>
	/// Loads the scene with the passed name.
	/// </summary>
	/// <param name="sceneName"></param>
	public static void LoadScene(string sceneName) {
		i.LoadSceneByName(sceneName);
	}

	/// <summary>
	/// Loads the scene with the passed build index.
	/// </summary>
	/// <param name="sceneIndex"></param>
	public static void LoadScene(int sceneIndex) {
		i.LoadSceneByIndex(sceneIndex);
	}

	/// <summary>
	/// Loads a scene by name after fading to black.
	/// </summary>
	/// <param name="sceneName"></param>
	void LoadSceneByName(string sceneName) {
		Teardown();
		
		LeanTween.value(gameObject, 0, 1, transitionAnimationLength / 2F).setEase(LeanTweenType.easeOutQuad).
			setOnUpdate((value) => { group.alpha = value; }).setOnComplete(
				() => {
					SceneManager.LoadScene(sceneName);
					
					Bootstrap();
					
					LeanTween.value(gameObject, 1, 0, transitionAnimationLength / 2F).
						setEase(LeanTweenType.easeInQuad).
						setOnUpdate((value) => { group.alpha = value; });
				});
	}

	/// <summary>
	/// Loads a scene by index object after fading to black.
	/// </summary>
	/// <param name="sceneIndex"></param>
	void LoadSceneByIndex(int sceneIndex) {
		Teardown();
		
		LeanTween.value(gameObject, 0, 1, transitionAnimationLength / 2F).setEase(LeanTweenType.easeOutQuad).
			setOnUpdate((value) => { group.alpha = value; }).setOnComplete(
				() => {
					SceneManager.LoadScene(sceneIndex);
					
					Bootstrap();
					
					LeanTween.value(gameObject, 1, 0, transitionAnimationLength / 2F).
						setEase(LeanTweenType.easeInQuad).
						setOnUpdate((value) => { group.alpha = value; });
				});
	}
	
	/// <summary>
	/// Runs before the scene is transitioned.
	/// </summary>
	void Teardown() {
		GameManager.Instance().Teardown();
		Transitioning = true;
	}

	/// <summary>
	/// Runs after the scene is transitioned.
	/// </summary>
	void Bootstrap() {
		GameManager.Instance().Bootstrap();
		Transitioning = false;
	}
	
	/// <summary>
	/// Fades the screen to black and quits the application.
	/// </summary>
	public static void Quit() {
		LeanTween.value(i.gameObject, 0, 1, transitionAnimationLength / 2F).
			setOnUpdate((value) => { i.group.alpha = value; }).setOnComplete(Application.Quit);
	}
}