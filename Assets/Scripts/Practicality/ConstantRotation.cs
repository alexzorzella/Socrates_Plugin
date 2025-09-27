using UnityEngine;

public class ConstantRotation : MonoBehaviour {
    public Vector3 rotate;

    void Update() {
        transform.Rotate(rotate * Time.deltaTime);
    }
}