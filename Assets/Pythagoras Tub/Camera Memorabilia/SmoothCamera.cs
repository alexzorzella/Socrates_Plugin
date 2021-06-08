using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{
    public static SmoothCamera i;

    [Header("Single Target Camera")]
    public float smoothTime;
    private Vector2 velocity;
    private Vector2 mousePad;
    private Vector2 clampPositions;
    private Vector2 gamepadExpandPosition;

    Transform target;

    private Camera cam;
    Animator anim;

    private void Start()
    {
        i = this;
        mousePad = new Vector2(0.3F, 0.3F);
        clampPositions = new Vector2(2.4F, 4.3F);
        gamepadExpandPosition = new Vector2(1, 1);
        cam = GetComponent<Camera>();
        anim = GetComponent<Animator>();
    }

    public static void Shake()
    {
        i.anim.SetTrigger("shake");
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
        if(target == null)
        {
            return;
        }

        MoveCamera();
    }

    private void MoveCamera()
    {
        AlexianInput input = null;
        //AlexianInput input = FindObjectOfType<ZPlayer_Sharon>().input; < Set the input here
        Vector2 mousePosRelativeToCamera = Vector2.zero;

        if (input.playerInput.currentControlScheme == "Gamepad")
        {
            mousePosRelativeToCamera = input.RightStick() * gamepadExpandPosition;
        }
        else if (input.playerInput.currentControlScheme == "K&B")
        {
            mousePosRelativeToCamera = input.MouseWorldPosition() - (Vector2)gameObject.transform.position;
        mousePosRelativeToCamera *= mousePad;
            mousePosRelativeToCamera = new Vector2(
    Mathf.Clamp(mousePosRelativeToCamera.x, -clampPositions.x, clampPositions.x),
    Mathf.Clamp(mousePosRelativeToCamera.y, -clampPositions.y, clampPositions.y));
        }

        if(target.tag != "Player")
        {
            mousePosRelativeToCamera = Vector2.zero;
        }

        float posX = Mathf.SmoothDamp(transform.position.x, target.position.x + mousePosRelativeToCamera.x, ref velocity.x, smoothTime);
        float posY = Mathf.SmoothDamp(transform.position.y, target.position.y + mousePosRelativeToCamera.y, ref velocity.y, smoothTime);

        transform.position = new Vector3(posX, posY, -10);
    }
}