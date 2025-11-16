using UnityEngine;
using UnityEngine.SceneManagement;

public class NATransition : MonoBehaviour {
	const float transitionLength = 0.5F;

	CanvasGroup group;

	private static NATransition _i;

	public static NATransition i {
		get {
			if (_i == null) {
				NATransition x = Resources.Load<NATransition>("Transition");
				_i = Instantiate(x);
			}

			return _i;
		}
	}

	private void Awake() {
		DontDestroyOnLoad(gameObject);
		group = GetComponent<CanvasGroup>();
		group.alpha = 0;
	}

	public static void QuitGame() {
		i.Quit();
	}

	public void LoadScene(string sceneName) {
		PreTransition();

		LeanTween.value(gameObject, 0, 1, transitionLength / 2F).setEase(LeanTweenType.easeOutQuad).
			setOnUpdate((value) => { group.alpha = value; }).setOnComplete(
				() => {
					SceneManager.LoadScene(sceneName);
					LeanTween.value(gameObject, 1, 0, transitionLength / 2F).setEase(LeanTweenType.easeInQuad).
					setOnUpdate((value) => { group.alpha = value; });
					// PostTransition();
				});
	}

	public void LoadScene(int sceneIndex) {
		PreTransition();

		LeanTween.value(gameObject, 0, 1, transitionLength / 2F).setEase(LeanTweenType.easeOutQuad).
			setOnUpdate((value) => { group.alpha = value; }).setOnComplete(
				() => {
					SceneManager.LoadScene(sceneIndex);
					LeanTween.value(gameObject, 1, 0, transitionLength / 2F).setEase(LeanTweenType.easeInQuad).
					setOnUpdate((value) => { group.alpha = value; });
					// PostTransition();
				});
	}

	public void Quit() {
		LeanTween.value(gameObject, 0, 1, transitionLength / 2F).setOnUpdate((value) => { group.alpha = value; }).setOnComplete(
		() => { Application.Quit(); });
	}

	void PreTransition() {
		GameManager.Instance().Bootstrap();
	}

	void PostTransition() {
		GameManager.Instance().Teardown();
	}
}