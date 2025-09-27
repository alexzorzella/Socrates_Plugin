using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScottsPhysicsCalc : MonoBehaviour
{
    Rigidbody2D rb;

    [Header("Steven's Data")]
    [Range(0f, 1f)]
    public float dampenBasic;
    [Range(0f, 1f)]
    public float dampenWhenStop;
    [Range(0f, 1f)]
    public float dampenWhenTurn;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void HorizontalDynamicDamp(float inputDirection, float speed)
    {
        float fHorizontalVelocity = rb.linearVelocity.x;

        float moveDirection = inputDirection;

        fHorizontalVelocity += moveDirection * speed;

        if (Mathf.Abs(moveDirection) < 0.01F)
        {
            fHorizontalVelocity *= Mathf.Pow(1f - dampenWhenStop, Time.deltaTime * 10f);
        }
        else if (Mathf.Sign(moveDirection) != Mathf.Sign(fHorizontalVelocity))
        {
            fHorizontalVelocity *= Mathf.Pow(1f - dampenWhenTurn, Time.deltaTime * 10f);
        }
        else
        {
            fHorizontalVelocity *= Mathf.Pow(1f - dampenBasic, Time.deltaTime * 10f);
        }

        rb.linearVelocity = new Vector2(fHorizontalVelocity, rb.linearVelocity.y);
    }
}