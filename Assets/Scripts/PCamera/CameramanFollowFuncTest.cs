using UnityEngine;

public class CameramanFollowFuncTest : MonoBehaviour {
    void Start() {
        PCamera.i.SetTargetWithTransform(transform);
    }
}