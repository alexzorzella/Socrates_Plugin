using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {
    readonly List<CameraShakeProfile> activeShakes = new();
    Vector3 offset;

    void Update() {
        UpdateCameraPosition();
    }

    public void SetAddedPosition(Vector3 position) {
        offset = position;
    }

    public void Shake(CameraShakeProfile profile) {
        activeShakes.Add(profile);
    }

    void UpdateCameraPosition() {
        PassThroughShakes();
    }

    void PassThroughShakes() {
        var decayedShakes = new List<CameraShakeProfile>();

        var finalCameraPos = Vector3.zero;
        var finalCameraRotation = Quaternion.identity;
        float zRotation = 0;

        foreach (var shake in activeShakes) {
            shake.UpdateIntensity();

            if (!shake.Decayed()) {
                finalCameraPos += shake.GetChoreography().position;
                finalCameraRotation *= shake.GetChoreography().rotation;
                zRotation += shake.GetChoreography().zRotation;
            } else {
                decayedShakes.Add(shake);
            }
        }

        transform.localPosition = new Vector3(finalCameraPos.x, finalCameraPos.y, -10) + offset;
        //transform.localRotation = Quaternion.Euler(new Vector3(0, 0, finalCameraRotation.z));
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, zRotation));

        foreach (var shake in decayedShakes) {
            activeShakes.Remove(shake);
        }
    }
}