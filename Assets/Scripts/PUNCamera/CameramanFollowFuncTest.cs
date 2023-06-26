using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameramanFollowFuncTest : MonoBehaviour
{
    private void Start()
    {
        PUNCamera.i.SetTargetWithTransform(transform);
    }
}