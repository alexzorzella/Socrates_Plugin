using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameramanTimothy : MonoBehaviour
{
    public static CameramanTimothy i;

    public bool applyKeyboardDirectionalView;
    public Vector2 directionalFactor;
    public bool applyMouseMultiplier;
    public float smoothTime;
    private Vector2 velocity;
    public Vector2 mouseMultiplier;
    public Vector2 clampPositions;
    public Vector2 offset;

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

    public void SetTargetWithTag(string targetTag = "Player")
    {
        if (GameObject.FindGameObjectWithTag(targetTag) == null)
            return;

        target = GameObject.FindGameObjectWithTag(targetTag).transform;
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
        //Vector2 mousePosRelativeToCamera = Vector2.zero;
        Vector2 directionalPrediction = Vector2.zero;
        
        if(applyKeyboardDirectionalView)
        {
            directionalPrediction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            directionalPrediction *= directionalFactor;
        }

        Vector2 targetPosition = (Vector2)target.position + directionalPrediction + offset;

//        mousePosRelativeToCamera = cam.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;

//        mousePosRelativeToCamera *= mouseMultiplier * (applyMouseMultiplier ? 1 : 0);
//        mousePosRelativeToCamera = new Vector2(
//Mathf.Clamp(mousePosRelativeToCamera.x, -clampPositions.x, clampPositions.x),
//Mathf.Clamp(mousePosRelativeToCamera.y, -clampPositions.y, clampPositions.y));

        float posX = Mathf.SmoothDamp(transform.position.x, targetPosition.x, ref velocity.x, smoothTime);
        float posY = Mathf.SmoothDamp(transform.position.y, targetPosition.y + directionalPrediction.y, ref velocity.y, smoothTime);

        //cameraShake.SetAddedPosition(directionalPrediction);
        transform.position = new Vector3(posX, posY, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, new Vector3(clampPositions.x, clampPositions.y, 0));
    }

    public void ResetOffset()
    {
        offset = Vector2.zero;
    }

    public void SetOffset(Vector2 offset)
    {
        this.offset = offset;
    }
}