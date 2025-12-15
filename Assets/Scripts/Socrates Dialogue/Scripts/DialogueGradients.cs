using UnityEngine;

public class DialogueGradients : MonoBehaviour {
    static DialogueGradients _i;

    public Gradient rainbow;

    void Start() {
        DontDestroyOnLoad(gameObject);
    }
	
    public static DialogueGradients i {
        get {
            if (_i == null) {
                DialogueGradients x = Resources.Load<DialogueGradients>("DialogueGradients");

                _i = Instantiate(x);
            }
            return _i;
        }
    }
}