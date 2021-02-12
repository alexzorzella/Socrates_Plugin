using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatioScale : MonoBehaviour
{
    public Camera mainCamera;
    public float ratio;
    public float multiplier;

    private void Start()
    {
        ratio = mainCamera.orthographicSize / transform.localScale.x;
    }

    private void Update()
    {
        float ratioMath = ratio * mainCamera.orthographicSize * multiplier;
        transform.localScale = new Vector3(ratioMath, ratioMath, ratioMath);
    }
}