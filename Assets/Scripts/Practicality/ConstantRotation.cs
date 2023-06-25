using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantRotation : MonoBehaviour
{
    public Vector3 rotate;

    private void Update()
    {
        transform.Rotate(rotate * Time.deltaTime);
    }
}