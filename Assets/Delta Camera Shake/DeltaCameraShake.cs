using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaCameraShake : MonoBehaviour
{
    [Range(0F, 1F)]
    public float jitterRandXY = 0.25F;
    [Range(0F, 1F)]
    public float jitterRandZ = 0.25F;
    [Range(0F, 10F)]
    public float matrixRand = 5;

    [Range(0F, 10F)]
    public float AngleMultiplier = 1.0F;
    [Range(0F, 10F)]
    public float CurveScale = 5;
    [Range(0F, 10F)]
    public float loopCount = 1.0F;

    [Range(0, 1F)]
    public float intensity = 0F;
    [Range(0, 1F)]
    public float shake_length;

    [Range(1F, 1.2F)]
    public float decay = 1.3F;

    [Range(10F, 50F)]
    public float initial_intensity = 20F;

    private struct Point
    {
        public float angleRange;
        public float angle;
        public float speed;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            intensity += initial_intensity;
        }

        //intensity = Mathf.Clamp(intensity - Time.deltaTime * 1 / shake_length, 0, float.MaxValue);
        intensity = intensity / decay; //Mathf.Clamp(intensity - Time.deltaTime * 1 / shake_length, 0, float.MaxValue);
        if (intensity < 0.001)
        {
            intensity = 0;
        }

        Shake();
    }

    private void Shake()
    {
        Point point = new Point();

        for (int i = 0; i < 1024; i++)
        {
            point.angleRange = UnityEngine.Random.Range(10f, 25f);
            point.speed = UnityEngine.Random.Range(1f, 3f);
        }

        Vector3 sourcePosition = transform.position;

        point.angle = Mathf.SmoothStep(-point.angleRange, point.angleRange, Mathf.PingPong(loopCount / 25f * point.speed, 1f));
        Vector3 jitterOffset = new Vector3(UnityEngine.Random.Range(-jitterRandXY, jitterRandXY), UnityEngine.Random.Range(-jitterRandXY, jitterRandXY), UnityEngine.Random.Range(-jitterRandZ, jitterRandZ));
        Matrix4x4 matrix = Matrix4x4.TRS(intensity * jitterOffset * CurveScale, Quaternion.Euler(0, 0, UnityEngine.Random.Range(-matrixRand, matrixRand) * AngleMultiplier), Vector3.one);

        transform.position = matrix.MultiplyPoint3x4(new Vector3(0, 0, -10));
        transform.rotation = Quaternion.Euler(jitterOffset * intensity);
    }
}