using UnityEngine;

public class ScottPhysics : MonoBehaviour {
    [Range(0f, 1f)]
    public float dampenBasic;

    [Range(0f, 1f)] public float dampenWhenStop;
    [Range(0f, 1f)] public float dampenWhenTurn;
    Rigidbody2D rb;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    public void HorizontalDynamicDamp(float inputDirection, float speed) {
        var fHorizontalVelocity = rb.linearVelocity.x;

        var moveDirection = inputDirection;

        fHorizontalVelocity += moveDirection * speed;

        if (Mathf.Abs(moveDirection) < 0.01F) {
            fHorizontalVelocity *= Mathf.Pow(1f - dampenWhenStop, Time.deltaTime * 10f);
        } else if (!Mathf.Approximately(Mathf.Sign(moveDirection), Mathf.Sign(fHorizontalVelocity))) {
            fHorizontalVelocity *= Mathf.Pow(1f - dampenWhenTurn, Time.deltaTime * 10f);
        } else {
            fHorizontalVelocity *= Mathf.Pow(1f - dampenBasic, Time.deltaTime * 10f);
        }

        rb.linearVelocity = new Vector2(fHorizontalVelocity, rb.linearVelocity.y);
    }
}