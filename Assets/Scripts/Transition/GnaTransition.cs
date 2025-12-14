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
		Scene scene = SceneManager.GetSceneByName(sceneName);
		i.LoadScene(scene);
	}

	/// <summary>
	/// Loads the scene with the passed build index.
	/// </summary>
	/// <param name="sceneIndex"></param>
	public static void LoadScene(int sceneIndex) {
		Scene scene = SceneManager.GetSceneByBuildIndex(sceneIndex);
		i.LoadScene(scene);
	}

	/// <summary>
	/// Loads a scene by scene object after fading to black.
	/// </summary>
	/// <param name="scene"></param>
	void LoadScene(Scene scene) {
		PreTransition();

		LeanTween.value(gameObject, 0, 1, transitionAnimationLength / 2F).setEase(LeanTweenType.easeOutQuad).
			setOnUpdate((value) => { group.alpha = value; }).setOnComplete(
				() => {
					SceneManager.LoadScene(scene.name);
					LeanTween.value(gameObject, 1, 0, transitionAnimationLength / 2F).
						setEase(LeanTweenType.easeInQuad).
						setOnUpdate((value) => { group.alpha = value; });
					PostTransition();
				});
	}
	
	/// <summary>
	/// Runs before the scene is transitioned.
	/// </summary>
	void PreTransition() {
		GameManager.Instance().Teardown();
	}

	/// <summary>
	/// Runs after the scene is transitioned.
	/// </summary>
	void PostTransition() {
		GameManager.Instance().Bootstrap();
	}
	
	/// <summary>
	/// Fades the screen to black and quits the application.
	/// </summary>
	public static void Quit() {
		LeanTween.value(i.gameObject, 0, 1, transitionAnimationLength / 2F).
			setOnUpdate((value) => { i.group.alpha = value; }).setOnComplete(Application.Quit);
	}
}