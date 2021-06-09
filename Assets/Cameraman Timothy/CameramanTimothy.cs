using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameramanTimothy : MonoBehaviour
{
    public static CameramanTimothy i;

    public bool applyMouseMultiplier;
    public float smoothTime;
    private Vector2 velocity;
    public Vector2 mouseMultiplier;
    public Vector2 clampPositions;

    Transform target;

    private Camera cam;

    private DeltaCameraShake cameraShake;

    private void Awake()
    {
        i = this;
        cam = GetComponentInChildren<Camera>();
        cameraShake = GetComponentInChildren<DeltaCameraShake>();
    }

    public static DeltaCameraShake GetShake()
    {
        return i.cameraShake;
    }

    public void SetTargetWithTag(string targetName = "Player")
    {
        target = GameObject.FindGameObjectWithTag(targetName).transform;
    }

    public void SetTargetWithTransform(Transform targetT)
    {
        if (targetT == null)
        {
            Debug.Log($"{target} not found.");
            return;
        }

        target = targetT;
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector2 mousePosRelativeToCamera = Vector2.zero;

        mousePosRelativeToCamera = cam.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;

        mousePosRelativeToCamera *= mouseMultiplier * (applyMouseMultiplier ? 1 : 0);
        mousePosRelativeToCamera = new Vector2(
Mathf.Clamp(mousePosRelativeToCamera.x, -clampPositions.x, clampPositions.x),
Mathf.Clamp(mousePosRelativeToCamera.y, -clampPositions.y, clampPositions.y));

        float posX = Mathf.SmoothDamp(transform.position.x, target.position.x, ref velocity.x, smoothTime);
        float posY = Mathf.SmoothDamp(transform.position.y, target.position.y, ref velocity.y, smoothTime);

        cameraShake.SetAddedPosition(mousePosRelativeToCamera);
        transform.position = new Vector3(posX, posY, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(clampPositions.x, clampPositions.y, 0));
    }
}