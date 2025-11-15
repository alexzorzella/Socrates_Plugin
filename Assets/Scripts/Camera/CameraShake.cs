using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour {
    public List<CameraShakeProfile> activeShakes = new();
    Vector3 addedPosition;

    void Update() {
        UpdateCameraPosition();
    }

    public void SetAddedPosition(Vector3 position) {
        addedPosition = position;
    }

    public void Shake(CameraShakeProfile profile) {
        activeShakes.Add(profile);
    }

    void UpdateCameraPosition() {
        PassThroughShakes();
    }

    void PassThroughShakes() {
        var validShakes = new List<CameraShakeProfile>();

        var finalCameraPos = Vector3.zero;
        var finalCameraRotation = Quaternion.identity;
        float zRotation = 0;

        foreach (var activeShake in activeShakes) {
            activeShake.UpdateIntensity();

            if (!activeShake.Decayed()) {
                validShakes.Add(activeShake);
                finalCameraPos += activeShake.GetChoreography().position;
                finalCameraRotation *= activeShake.GetChoreography().rotation;
                zRotation += activeShake.GetChoreography().zRotation;
            }
        }

        transform.localPosition = new Vector3(finalCameraPos.x, finalCameraPos.y, -10) + addedPosition;
        //transform.localRotation = Quaternion.Euler(new Vector3(0, 0, finalCameraRotation.z));
        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, zRotation));

        activeShakes = validShakes;
    }
}