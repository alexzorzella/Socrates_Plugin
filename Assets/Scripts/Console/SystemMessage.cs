using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SystemMessage : MonoBehaviour {
	public const float lifetime = 1.2F;
	public const float fadeSpeed = 1F;

	float currentLifetime;
	float fadeTime;

	RectTransform rect;
	public TextMeshProUGUI text;

	private void Awake() {
		currentLifetime = lifetime;
		rect = GetComponent<RectTransform>();
	}

	public void SetText(string content) {
		text.text = content;
	}

	public void Update() {
		if (text.color.a <= 0) {
			JConsole.i.UpdateCurrentMessages(-1, rect.sizeDelta.y);
			Destroy(gameObject);
		}

		if (currentLifetime <= 0) {
			text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(1, 0, fadeTime));
			fadeTime += Time.deltaTime * fadeSpeed;
		} else {
			currentLifetime -= Time.deltaTime;
		}
	}
}