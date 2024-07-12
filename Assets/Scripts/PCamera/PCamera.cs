using UnityEngine;

public class PCamera : MonoBehaviour
{
    public static PCamera i;

    public bool applyDirectionalInputOffset;
    public Vector2 directionalFactor;
    public bool applyMouseOffset;
    public Vector2 mouseMultiplier;
    public Vector2 clampPositions;

    public float smoothTime;

    private Vector2 velocity;

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

    public void SetTargetWithTransform(Transform newTransform)
    {
        if (newTransform == null)
            return;

        target = newTransform;
    }

    private void Update()
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        Vector2 directionalPrediction = Vector2.zero;
        
        if(applyDirectionalInputOffset)
        {
            directionalPrediction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            directionalPrediction *= directionalFactor;
        }

        Vector2 mousePosRelativeToCamera = Vector2.zero;

        if (applyMouseOffset)
        {
            mousePosRelativeToCamera = cam.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;

            mousePosRelativeToCamera *= mouseMultiplier;

            mousePosRelativeToCamera = new Vector2(
                Mathf.Clamp(mousePosRelativeToCamera.x, -clampPositions.x, clampPositions.x),
                Mathf.Clamp(mousePosRelativeToCamera.y, -clampPositions.y, clampPositions.y));
        }

        Vector2 targetPosition = target != null ? (Vector2)target.position : Vector2.zero;

        Vector2 finalPosition = targetPosition + directionalPrediction;

        float posX = Mathf.SmoothDamp(transform.position.x, finalPosition.x + directionalPrediction.x, ref velocity.x, smoothTime);
        float posY = Mathf.SmoothDamp(transform.position.y, finalPosition.y + directionalPrediction.y, ref velocity.y, smoothTime);

        if (cameraShake != null)
        {
            cameraShake.SetAddedPosition(mousePosRelativeToCamera);
        }

        transform.position = new Vector2(posX, posY);
    }
}