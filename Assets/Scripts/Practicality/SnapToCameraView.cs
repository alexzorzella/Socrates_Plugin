using UnityEngine;

public class SnapToCameraView : MonoBehaviour {
    enum Mode { UPPER_LEFT, UPPER_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT }
    
    readonly Vector2 offset = new Vector2(-1.75F, 0F);
    const float zValue = 2.4F;

    void Start() {
        SnapToView();
    }

    void SnapToView() {
        Camera cam = Camera.main;

        if (cam != null) {
            float frustumHeight = 2F * zValue * Mathf.Tan(cam.fieldOfView * 0.5F * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * cam.aspect;

            Vector3 localBottomRight = new Vector3(
                frustumWidth / 2F + offset.x,
                -frustumHeight / 2F + offset.y,
                zValue);

            transform.localPosition = localBottomRight;
        }
    }
}