using System;
using UnityEngine;

public class GameAssets : MonoBehaviour {
	static GameAssets _i;

	void Start() {
		DontDestroyOnLoad(gameObject);
	}

	public Texture Socrates;

	public static GameAssets i {
		get {
			if (_i == null) {
				GameAssets x = Resources.Load<GameAssets>("GameAssets");

				_i = Instantiate(x);
			}
			return _i;
		}
	}

	public RuntimeAnimatorController LoadAnimatorController(string animatorName) {
		RuntimeAnimatorController result = Resources.Load<RuntimeAnimatorController>(animatorName);

		if (result == null) {
			Debug.Log($"Controlador de animação de '{animatorName}' não existe.");
			return null;
		}

		return result;
	}

	public GameObject LoadObject(string objectName) {
		GameObject result = Resources.Load<GameObject>(objectName);
		return result;
	}

	public GameObject InstantiateObject(string objectName, Vector2 position, Quaternion rotation) {
		GameObject loadedObject = LoadObject(objectName);

		if (loadedObject == null) {
			return null;
		}

		return Instantiate(loadedObject, position, rotation);
	}

	public Sprite LoadAsepriteSprite(string name) {
		var asepriteFile = Resources.Load<GameObject>(name);

		Sprite result = null;

		if (asepriteFile != null) {
			result = asepriteFile.GetComponent<SpriteRenderer>().sprite;
		}

		return result;
	}

	public Sprite LoadSprite(string name) {
		Sprite result = Resources.Load<Sprite>(name);

		if (result == null) {
			Debug.LogWarning($"Não existe um sprite no seus Recursos chamado '{name}'.");
			return null;
		}

		return result;
	}

	public Sprite LoadSprite(string mainTexture, string subTextureName) {
		Sprite[] sprites = Resources.LoadAll<Sprite>(mainTexture);

		if (sprites != null) {
			Sprite sprite = Array.Find(sprites, s => s.name == subTextureName);
			return sprite;
		}

		return null;
	}

	public Sprite LoadFirstSprite(string mainTexture) {
		Sprite[] sprites = Resources.LoadAll<Sprite>(mainTexture);

		if (sprites != null) {
			if (sprites.Length > 0) {
				Sprite sprite = sprites[0];
				return sprite;
			}
		}

		return null;
	}

	public Sprite LoadRandomSprite(string mainTexture) {
		Sprite[] sprites = Resources.LoadAll<Sprite>(mainTexture);

		if (sprites != null) {
			if (sprites.Length > 0) {
				Sprite sprite = sprites[UnityEngine.Random.Range(0, sprites.Length)];
				return sprite;
			}
		}

		return null;
	}

	public Vector2 ProportionalSize(float width, float height, float targetSize, bool anchorWidth = true) {
		Vector2 result = Vector2.zero;

		if(anchorWidth) {
			result = new Vector2(targetSize, (height / width) * targetSize);
		} else {
			result = new Vector2((width / height) * targetSize, targetSize);
		}

		return result;
	}

	const int compressTo = 42;
	public enum AlexianCompressionMode { SMOOTH, QUICK }

	public Sprite CompressSprite(string name, AlexianCompressionMode compressionMode = AlexianCompressionMode.SMOOTH) {
		Sprite toCompress = LoadSprite(name);

		if (toCompress == null) {
			return null;
		}

		float actualWidth = toCompress.texture.width;
		float actualHeight = toCompress.texture.height;

		bool horizontal = actualWidth > actualHeight;
		int targetWidth = compressTo;
		int targetHeight = compressTo;

		/*
		actualWidth     targetWidth
		-----------  =  ------------
		actualHeight    targetHeight
		*/
		if (horizontal) {
			targetWidth = (int)(compressTo * (actualWidth / actualHeight));
		} else {
			targetHeight = (int)(compressTo * (actualHeight / actualWidth));
		}

		Texture2D texture = new Texture2D(targetWidth, targetHeight);
		texture.filterMode = FilterMode.Point;

		for (int x = 0; x < texture.width; x++) {
			for (int y = 0; y < texture.height; y++) {
				texture.SetPixel(x, y, new Color(1, 1, 1, 0));
			}
		}

		int incrementX = (int)(actualWidth / targetWidth);
		int incrementY = (int)(actualHeight / targetHeight);

		for (int x = 0; x < targetWidth; x++) {
			for (int y = 0; y < targetHeight; y++) {
				Color finalColor = new Color();

				if (compressionMode == AlexianCompressionMode.SMOOTH) {
					float finalR = 0;
					float finalG = 0;
					float finalB = 0;

					for (int w = 0; w < incrementX; w++) {
						for (int h = 0; h < incrementY; h++) {
							Color pixel = toCompress.texture.GetPixel(x * incrementX + w, y * incrementY + h);

							finalR += pixel.r;
							finalG += pixel.g;
							finalB += pixel.b;
						}
					}

					int area = incrementX * incrementY;
					finalColor = new Color(finalR / area, finalG / area, finalB / area, 1);
				} else {
					finalColor = toCompress.texture.GetPixel(x * incrementX, y * incrementY);
				}

				texture.SetPixel(x, y, finalColor);
			}
		}

		texture.Apply();

		Sprite result = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5F, 0.5F), compressTo);
		result.name = $"{name}_{compressionMode.ToString().ToLower()}ly_compressed_{targetWidth}x{targetHeight}";
		return result;
	}

	//Have a nice day.
}