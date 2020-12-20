using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float smoothTimeX = 0, smoothTimeY = 0;
    private Vector2 velocity;
    private GameObject target;

    private void Start()
    {
        SetTarget();
    }

    public void SetTarget(string targetName = "Player")
    {
        target = GameObject.FindGameObjectWithTag(targetName);
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        float posX = Mathf.SmoothDamp(transform.position.x, target.transform.position.x, ref velocity.x, smoothTimeX);
        float posY = Mathf.SmoothDamp(transform.position.y, target.transform.position.y, ref velocity.y, smoothTimeY);
    }
}