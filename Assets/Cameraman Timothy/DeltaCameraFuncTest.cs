using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaCameraFuncTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            FindObjectOfType<DeltaCameraShake>().Shake(Shakepedia.GetProfileClone(Shakepedia.MINOR));
        } else if(Input.GetMouseButtonDown(1))
        {
            FindObjectOfType<DeltaCameraShake>().Shake(Shakepedia.GetProfileClone(Shakepedia.MILD));
        }
    }
}