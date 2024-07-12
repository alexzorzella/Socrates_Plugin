using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeltaCameraFuncTest : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !FindObjectOfType<DialogueManager>().Talking())
        {
			Debug.Log($"Should shake");
            FindObjectOfType<DeltaCameraShake>().Shake(Shakepedia.GetProfileClone(Shakepedia.MINOR));
        }
    }
}