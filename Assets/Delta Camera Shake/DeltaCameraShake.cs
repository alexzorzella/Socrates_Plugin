using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaCameraShake : MonoBehaviour
{
    public List<ShakeProfile> activeShakes = new List<ShakeProfile>();

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            activeShakes.Add(Shakepedia.GetProfileClone(Shakepedia.CLASSIC));
        }

        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        PassThroughShakes();
    }

    private void PassThroughShakes()
    {
        List<ShakeProfile> validShakes = new List<ShakeProfile>();

        Vector3 finalCameraPos = Vector3.zero;
        Quaternion finalCameraRotation = Quaternion.identity;

        foreach (var activeShake in activeShakes)
        {
            activeShake.UpdateIntensity();

            if (!activeShake.Decayed())
            {
                validShakes.Add(activeShake);
                finalCameraPos += activeShake.GetCoreography().position;
                finalCameraRotation *= activeShake.GetCoreography().rotation;
            } 
        }

        transform.position = new Vector3(finalCameraPos.x, finalCameraRotation.y, -10);
        transform.rotation = finalCameraRotation;

        activeShakes = validShakes;
    }

    public class ShakeProfile
    {
        public ShakeProfile(
            float jitterRandXY,
            float jitterRandZ,
            float matrixRand,
            float angleMultiplier,
            float curveScale,
            float loopCount,
            float decay,
            float initialIntensity)
        {
            this.jitterRandXY = jitterRandXY;
            this.jitterRandZ = jitterRandZ;
            this.matrixRand = matrixRand;
            this.angleMultiplier = angleMultiplier;
            this.curveScale = curveScale;
            this.loopCount = loopCount;
            this.decay = decay;
            this.initialIntensity = initialIntensity;

            this.shakeStartTs = Time.realtimeSinceStartup;
            currentIntensity = this.initialIntensity;
        }

        public float jitterRandXY;
        public float jitterRandZ;
        public float matrixRand;
        public float angleMultiplier;
        public float curveScale;
        public float loopCount;

        public float shakeStartTs;
        public float currentIntensity;
        public float decay;

        public float initialIntensity;

        public void UpdateIntensity()
        {

            if (currentIntensity == 0) {
                return;
            }

            float elapsedTime = Time.realtimeSinceStartup - shakeStartTs;
            //currentIntensity = currentIntensity / decay * Time.deltaTime;
            currentIntensity = initialIntensity * Mathf.Pow((decay), elapsedTime);


            if (currentIntensity < 0.01)
            {
                currentIntensity = 0;
            }
        }

        public bool Decayed()
        {
            return currentIntensity < 0.01F;
        }

        private struct Point
        {
            public float angleRange;
            public float angle;
            public float speed;
        }

        public struct CameraMovementData
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        public CameraMovementData GetCoreography()
        {
            Point point = new Point();

            point.angleRange = UnityEngine.Random.Range(10f, 25f);
            point.speed = UnityEngine.Random.Range(1f, 3f);

            point.angle = Mathf.SmoothStep(-point.angleRange, point.angleRange, Mathf.PingPong(loopCount / 25f * point.speed, 1f));
            Vector3 jitterOffset = new Vector3(UnityEngine.Random.Range(-jitterRandXY, jitterRandXY),
                                               UnityEngine.Random.Range(-jitterRandXY, jitterRandXY),
                                               UnityEngine.Random.Range(-jitterRandZ, jitterRandZ));
            
            Matrix4x4 matrix = Matrix4x4.TRS(currentIntensity * jitterOffset * curveScale,
                                             Quaternion.Euler(0, 0, UnityEngine.Random.Range(-matrixRand, matrixRand) * angleMultiplier),
                                             Vector3.one);

            Vector3 matrixMult = matrix.MultiplyPoint3x4(new Vector3(0, 0, -10));

            CameraMovementData cmd = new CameraMovementData();

            cmd.position = new Vector3(matrixMult.x, matrixMult.y, -10);
            cmd.rotation = Quaternion.Euler(jitterOffset * currentIntensity);

            return cmd;
        }
    }

    public static class Shakepedia
    {
        public static ShakeProfile CLASSIC = new ShakeProfile(0.1F, 0.2F, 10F, 1F, 0.6F, 1F, 0.1F, 8F);
        public static ShakeProfile ONE_HUNDRED = new ShakeProfile(100F, 100F, 100F, 100F, 100F, 100F, 100F, 100F);

        public static ShakeProfile GetProfileClone(ShakeProfile profile)
        {
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
}