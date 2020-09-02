using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public string defaultTargetTag = "Player";
    public enum CameraType { OFF, SNAP, FOLLOW_BEHIND, PREDICT, ADAPT }
    public CameraType camType;

    [Header("Normal Settings")]
    public float smoothTimeX = 0, smoothTimeY = 0, addedDistance = 2;
    private Vector2 velocity_a;
    private GameObject target;
    
    [Header("Adapting Camera Settings")]

    public List<Transform> targets;
    public Vector3 offset;
    private Vector3 velocity;
    public float smoothTime;
    public float minZoom;
    public float maxZoom;
    public float zoomLimiter;
    private Camera cam;

    private void Start()
    {
        SetTarget();
    }

    public void AddTransform(Transform addition)
    {
        targets.Add(addition);
    }

    private void SetTarget()
    {
        target = GameObject.FindGameObjectWithTag(defaultTargetTag);
        cam = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector2 updatedTarget = new Vector2();

        switch (camType)
        {
            case CameraType.OFF:
                break;
            case CameraType.SNAP:
                smoothTimeX = 0; smoothTimeY = 0;
                break;
            case CameraType.FOLLOW_BEHIND:
                updatedTarget = target.transform.position;
                break;
            case CameraType.PREDICT:
                updatedTarget = new Vector2(
               (target.transform.position.x + moveInput.x * addedDistance),
               (target.transform.position.y + moveInput.y * addedDistance));
                break;
            case CameraType.ADAPT:
                break;
        }

        float posX = Mathf.SmoothDamp(transform.position.x, updatedTarget.x, ref velocity.x, smoothTimeX);
        float posY = Mathf.SmoothDamp(transform.position.y, updatedTarget.y, ref velocity.y, smoothTimeY);
    }

    private void LateUpdate()
    {
        if (targets.Count == 0)
        {
            return;
        }

        Move();
        Zoom();
    }

    void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime);
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();

        Vector3 newPosition = centerPoint + offset;

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector2.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return -bounds.size.x;
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector2.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }
}