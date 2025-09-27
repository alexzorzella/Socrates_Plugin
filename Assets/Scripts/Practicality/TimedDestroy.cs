using UnityEngine;

public class TimedDestroy : MonoBehaviour {
    public float time;

    void Start() {
        Destroy(gameObject, time);
    }
}