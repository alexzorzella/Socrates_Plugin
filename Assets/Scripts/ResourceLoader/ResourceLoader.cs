using System;
using UnityEngine;
using Object = UnityEngine.Object;

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

	/// <summary>
	/// Returns the path to the object given its name and the passed subfolder, accounting
	/// for the fact that there may be no subfolder passed.
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="subfolder"></param>
	/// <returns></returns>
	static string SafePath(string filename, string subfolder = "") {
		string result = "";

		if (string.IsNullOrWhiteSpace(subfolder)) {
			result = filename;
		} else {
			result = $"{subfolder}/{filename}";
		}

		return result;
	}

	/// <summary>
	/// Returns an object named objectName found in any Resources folder. If specified, the function
	/// will look into a subfolder. Multiple subfolders are supported. A slash is not required for
	/// the deepest folder.
	/// </summary>
	/// <param name="objectName">The name of the object to be loaded.</param>
	/// <param name="subfolder">The path of the object to be loaded. i.e. 'Cats' or 'Pets/Cats'.</param>
	/// <returns></returns>
	public static GameObject LoadObject(string objectName, string subfolder = "") {
		string path = SafePath(objectName, subfolder);
		
		GameObject result = Resources.Load<GameObject>(path);
		return result;
	}

	/// <summary>
	/// Instantiates an object with the passed name at the origin with no rotation if one is found.
	/// </summary>
	/// <param name="objectName"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public static GameObject InstantiateObject(string objectName, Transform parent = null) {
		GameObject result = InstantiateObject(objectName, Vector3.zero, Quaternion.identity, parent);
		return result;
	}
	
	/// <summary>
	/// Instantiates an object with the passed name at the given position with the given rotation if one is found.
	/// If a parent transform is passed, it instantiates it parented to that object.
	/// </summary>
	/// <param name="objectName"></param>
	/// <param name="position"></param>
	/// <param name="rotation"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public static GameObject InstantiateObject(string objectName, Vector3 position, Quaternion rotation, Transform parent = null) {
		GameObject loadedObject = LoadObject(objectName);

		if (loadedObject == null) {
			return null;
		}

		return Instantiate(loadedObject, position, rotation, parent);
	}

	/// <summary>
	/// Returns an object of type T named filename found in any Resources folder. If specified,
	/// the function will look into a subfolder. Multiple subfolders are supported. A slash is
	/// not required for the deepest folder.
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="subfolder"></param>
	/// <returns></returns>
	public T LoadFile<T>(string filename, string subfolder = "") where T : Object {
		string path = SafePath(filename, subfolder);
		
		T result = Resources.Load<T>(path);

		if (result == null) {
			Debug.Log($"No file found at {path}.");
			return null;
		}

		return result;
	}
	
	/// <summary>
	/// Returns an Aseprite file named filename found in any Resources folder. If specified, the function
	/// will look into a subfolder. Multiple subfolders are supported. A slash is not required for
	/// the deepest folder.
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="subfolder"></param>
	/// <returns></returns>
	public static Sprite LoadAsepriteSprite(string filename, string subfolder = "") {
		string path = SafePath(filename, subfolder);

		var asepriteFile = Resources.Load<GameObject>(path);

		Sprite result = null;

		if (asepriteFile != null) {
			result = asepriteFile.GetComponent<SpriteRenderer>().sprite;
		} else {
			Debug.LogWarning($"No file found at {path}.");
		}

		return result;
	}

	/// <summary>
	/// Returns a Sprite named filename found in any Resources folder. If specified, the function
	/// will look into a subfolder. Multiple subfolders are supported. A slash is not required for
	/// the deepest folder.
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="subfolder"></param>
	/// <returns></returns>
	public static Sprite LoadSprite(string filename, string subfolder = "") {
		string path = SafePath(filename, subfolder);
		
		Sprite result = Resources.Load<Sprite>(path);

		if (result == null) {
			Debug.LogWarning($"No sprite found at {path}.");
			return null;
		}

		return result;
	}

	/// <summary>
	/// Returns a subtexture Sprite named subtextureName whose parent Sprite is named mainTexture
	/// found in any Resources folder. If specified, the function will look into a subfolder.
	/// Multiple subfolders are supported. A slash is not required for the deepest folder.
	/// </summary>
	/// <param name="mainTexture"></param>
	/// <param name="subtextureName"></param>
	/// <param name="subfolder"></param>
	/// <returns></returns>
	public static Sprite LoadSprite(string mainTexture, string subtextureName, string subfolder = "") {
		string path = SafePath(mainTexture, subfolder);
		
		Sprite[] sprites = Resources.LoadAll<Sprite>(path);

		if (sprites != null) {
			Sprite sprite = Array.Find(sprites, s => s.name == subtextureName);
			return sprite;
		}
		
		Debug.LogError($"No subtexture named {subtextureName} found at {path}.");

		return null;
	}

	/// <summary>
	/// Returns the first subtexture Sprite whose parent Sprite is named mainTexture found
	/// in any Resources folder. If specified, the function will look into a subfolder.
	/// Multiple subfolders are supported. A slash is not required for the deepest folder.
	/// </summary>
	/// <param name="mainTexture"></param>
	/// <param name="subfolder"></param>
	/// <returns></returns>
	public static Sprite LoadFirstSprite(string mainTexture, string subfolder = "") {
		string path = SafePath(mainTexture, subfolder);
		
		Sprite[] sprites = Resources.LoadAll<Sprite>(path);

		if (sprites != null) {
			if (sprites.Length > 0) {
				Sprite sprite = sprites[0];
				return sprite;
			}
		}

		Debug.LogError($"No subtexture was found at {path}.");
		
		return null;
	}

	/// <summary>
	/// Returns a random subtexture Sprite whose parent Sprite is named mainTexture found
	/// in any Resources folder. If specified, the function will look into a subfolder.
	/// Multiple subfolders are supported. A slash is not required for the deepest folder.
	/// </summary>
	/// <param name="mainTexture"></param>
	/// <param name="subfolder"></param>
	/// <returns></returns>
	public static Sprite LoadRandomSprite(string mainTexture, string subfolder = "") {
		string path = SafePath(mainTexture, subfolder);
		
		Sprite[] sprites = Resources.LoadAll<Sprite>(path);

		if (sprites != null) {
			if (sprites.Length > 0) {
				Sprite sprite = sprites[UnityEngine.Random.Range(0, sprites.Length)];
				return sprite;
			}
		}
		
		Debug.LogError($"No subtexture was found at {path}.");

		return null;
	}

	//Have a nice day.
}