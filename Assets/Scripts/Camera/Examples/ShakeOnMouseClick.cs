using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShakeOnMouseClick : MonoBehaviour {
    CameraShake shake;
    
    void Start() {
        shake = FindFirstObjectByType<CameraShake>();
    }

    void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            shake.Shake(Shakepedia.GetProfileClone(Shakepedia.MINOR));
        }
    }
}