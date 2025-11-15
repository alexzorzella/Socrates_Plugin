using System;
using UnityEngine;

public class ResourceLoader : MonoBehaviour {
	static ResourceLoader _i;

	void Start() {
		DontDestroyOnLoad(gameObject);
	}
	
	public static ResourceLoader i {
		get {
			if (_i == null) {
				ResourceLoader x = Resources.Load<ResourceLoader>("ResourceLoader");

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
	
	public static GameObject LoadObject(string objectName) {
		GameObject result = Resources.Load<GameObject>(objectName);
		return result;
	}

	public static GameObject InstantiateObject(string objectName, Vector2 position, Quaternion rotation) {
		GameObject loadedObject = LoadObject(objectName);

		if (loadedObject == null) {
			return null;
		}

		return Instantiate(loadedObject, position, rotation);
	}

	public static Sprite LoadAsepriteSprite(string name) {
		var asepriteFile = Resources.Load<GameObject>(name);

		Sprite result = null;

		if (asepriteFile != null) {
			result = asepriteFile.GetComponent<SpriteRenderer>().sprite;
		}

		return result;
	}

	public static Sprite LoadSprite(string name) {
		Sprite result = Resources.Load<Sprite>(name);

		if (result == null) {
			Debug.LogWarning($"Não existe um sprite no seus Recursos chamado '{name}'.");
			return null;
		}

		return result;
	}

	public static Sprite LoadSprite(string mainTexture, string subTextureName) {
		Sprite[] sprites = Resources.LoadAll<Sprite>(mainTexture);

		if (sprites != null) {
			Sprite sprite = Array.Find(sprites, s => s.name == subTextureName);
			return sprite;
		}

		return null;
	}

	public static Sprite LoadFirstSprite(string mainTexture) {
		Sprite[] sprites = Resources.LoadAll<Sprite>(mainTexture);

		if (sprites != null) {
			if (sprites.Length > 0) {
				Sprite sprite = sprites[0];
				return sprite;
			}
		}

		return null;
	}

	public static Sprite LoadRandomSprite(string mainTexture) {
		Sprite[] sprites = Resources.LoadAll<Sprite>(mainTexture);

		if (sprites != null) {
			if (sprites.Length > 0) {
				Sprite sprite = sprites[UnityEngine.Random.Range(0, sprites.Length)];
				return sprite;
			}
		}

		return null;
	}

	//Have a nice day.
}