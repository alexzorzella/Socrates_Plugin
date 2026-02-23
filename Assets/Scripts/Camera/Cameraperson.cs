using UnityEngine;

public class Cameraperson : MonoBehaviour {
    static Cameraperson i;

    public static Cameraperson Get() {
        return i;
    }
    
    public bool applyDirectionalInputOffset;
    public Vector2 directionalFactor;
    public bool applyMouseOffset;
    public Vector2 mouseMultiplier;
    public Vector2 clampPositions;

    public float smoothTime;

    Camera cam;

    CameraShake cameraShake;

    Transform target;

    Vector2 velocity;

    void Awake() {
        i = this;
        cam = GetComponentInChildren<Camera>();
        cameraShake = GetComponentInChildren<CameraShake>();
    }

    void Update() {
        MoveCamera();
    }

    public static CameraShake GetShake() {
        return i.cameraShake;
    }

    public void SetTargetWithTag(string targetTag = "Player") {
        if (GameObject.FindGameObjectWithTag(targetTag) == null)
            return;

        target = GameObject.FindGameObjectWithTag(targetTag).transform;
    }

    public void SetTargetWithTransform(Transform newTransform) {
        if (newTransform == null)
            return;

        target = newTransform;
    }

    void MoveCamera() {
        var directionalPrediction = Vector2.zero;

        if (applyDirectionalInputOffset) {
            directionalPrediction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            directionalPrediction *= directionalFactor;
        }

        var mousePosRelativeToCamera = Vector2.zero;

        if (applyMouseOffset) {
            mousePosRelativeToCamera = cam.ScreenToWorldPoint(Input.mousePosition) - gameObject.transform.position;

            mousePosRelativeToCamera *= mouseMultiplier;

            mousePosRelativeToCamera = new Vector2(
                Mathf.Clamp(mousePosRelativeToCamera.x, -clampPositions.x, clampPositions.x),
                Mathf.Clamp(mousePosRelativeToCamera.y, -clampPositions.y, clampPositions.y));
        }

        var targetPosition = target != null ? (Vector2)target.position : Vector2.zero;

        var finalPosition = targetPosition + directionalPrediction;

        var posX = Mathf.SmoothDamp(transform.position.x, finalPosition.x + directionalPrediction.x, ref velocity.x,
            smoothTime);
        var posY = Mathf.SmoothDamp(transform.position.y, finalPosition.y + directionalPrediction.y, ref velocity.y,
            smoothTime);

        if (cameraShake != null) cameraShake.SetAddedPosition(mousePosRelativeToCamera);

        transform.position = new Vector2(posX, posY);
    }
}