using UnityEngine;

public class Parallax : MonoBehaviour {
    public float length, startPos;
    public GameObject cameraObject;
    public float parallaxEffect;

    public bool fixedY;
    float startY;

    void Start() {
        startPos = transform.position.x;

        if (GetComponent<SpriteRenderer>() != null) length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update() {
        var temp = cameraObject.transform.position.x * (1 - parallaxEffect);
        var distance = cameraObject.transform.position.x * parallaxEffect;
        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        if (temp > startPos + length)
            startPos += length;
        else if (temp < startPos - length) startPos -= length;
    }
}