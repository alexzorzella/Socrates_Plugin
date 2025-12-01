using UnityEngine;

public class CameraShakeProfile {
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

    public CameraShakeProfile(
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