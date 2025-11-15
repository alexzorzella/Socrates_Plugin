using System.Collections.Generic;
using UnityEngine;

public class DeltaCameraShake : MonoBehaviour {
    public List<ShakeProfile> activeShakes = new();
    Vector3 addedPosition;

    void Update() {
        UpdateCameraPosition();
    }

    public void SetAddedPosition(Vector3 position) {
        addedPosition = position;
    }

    public void Shake(ShakeProfile profile) {
        activeShakes.Add(profile);
    }

    void UpdateCameraPosition() {
        PassThroughShakes();
    }

    void PassThroughShakes() {
        var validShakes = new List<ShakeProfile>();

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

public class ShakeProfile {
    public float angleMultiplier;
    public float currentIntensity;
    public float curveScale;
    public float decay;

    public float initialIntensity;

    public float jitterRandXY;
    public float jitterRandZ;
    public float loopCount;
    public float matrixRand;

    public float shakeStartTs;

    public ShakeProfile(
        float jitterRandXY,
        float jitterRandZ,
        float matrixRand,
        float angleMultiplier,
        float curveScale,
        float loopCount,
        float decay,
        float initialIntensity) {
        this.jitterRandXY = jitterRandXY;
        this.jitterRandZ = jitterRandZ;
        this.matrixRand = matrixRand;
        this.angleMultiplier = angleMultiplier;
        this.curveScale = curveScale;
        this.loopCount = loopCount;
        this.decay = decay;
        this.initialIntensity = initialIntensity;

        shakeStartTs = Time.realtimeSinceStartup;
        currentIntensity = this.initialIntensity;
    }

    public void UpdateIntensity() {
        if (currentIntensity == 0) return;

        var elapsedTime = Time.realtimeSinceStartup - shakeStartTs;
        currentIntensity = initialIntensity * Mathf.Pow(decay, elapsedTime);


        if (currentIntensity < 0.01) currentIntensity = 0;
    }

    public bool Decayed() {
        return currentIntensity < 0.01F;
    }

    public CameraMovementData GetChoreography() {
        var point = new Point();

        point.angleRange = Random.Range(5F, 10F);
        point.speed = Random.Range(1f, 3f);

        var favorRight = Random.Range(0, 1F) > 0.5F;

        if (favorRight) point.angleRange *= -1;

        point.angle = Mathf.SmoothStep(-point.angleRange, point.angleRange,
            Mathf.PingPong(loopCount / 25f * point.speed, 1f));

        var jitterOffset = new Vector3(Random.Range(-jitterRandXY, jitterRandXY),
            Random.Range(-jitterRandXY, jitterRandXY),
            Random.Range(-jitterRandZ, jitterRandZ));

        var matrix = Matrix4x4.TRS(currentIntensity * jitterOffset * curveScale,
            Quaternion.Euler(0, 0, Random.Range(-matrixRand, matrixRand) * angleMultiplier),
            Vector3.one);

        var matrixMult = matrix.MultiplyPoint3x4(new Vector3(0, 0, -10));

        var cmd = new CameraMovementData();

        cmd.position = new Vector3(matrixMult.x, matrixMult.y, -10);
        cmd.rotation = Quaternion.Euler(jitterOffset * currentIntensity);
        cmd.zRotation = point.angle * currentIntensity / 15F;

        return cmd;
    }

    struct Point {
        public float angleRange;
        public float angle;
        public float speed;
    }

    public struct CameraMovementData {
        public Vector3 position;
        public Quaternion rotation;
        public float zRotation;
    }
}

public static class Shakepedia {
    public static ShakeProfile MINOR = new(0.08F, 0.8F, 5F, 1F, 0.1F, 1F, 0.003F, 3F);

    public static ShakeProfile MILD = new(0.08F, 10F, 5F, 1F, 0.6F, 1F, 0.003F, 3F);

    public static ShakeProfile MEDIUM_RARE = new(0.1F, 0.2F, 10F, 1F, 0.6F, 1F, 0.003F, 6F);
    public static ShakeProfile POW = new(0.1F, 0.6F, 10F, 10F, 0.6F, 1F, 0.003F, 11F);

    public static ShakeProfile RUMBLE = new(0.5F, 0.5F, 0.5F, 0.5F, 0.5F, 0.5F, 1F, 0.05F);

    public static ShakeProfile GetProfileClone(ShakeProfile profile) {
        return new ShakeProfile(
            profile.jitterRandXY,
            profile.jitterRandZ,
            profile.matrixRand,
            profile.angleMultiplier,
            profile.curveScale,
            profile.loopCount,
            profile.decay,
            profile.initialIntensity);
    }
}