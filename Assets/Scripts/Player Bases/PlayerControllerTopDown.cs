using UnityEngine;

public class PlayerControllerTopDown : MonoBehaviour {
    public float speed;
    Vector2 moveVelocity;

    Rigidbody2D rb;

    void Start() {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update() {
        var moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveVelocity = moveInput * speed;
    }

    void FixedUpdate() {
        rb.MovePosition(rb.position + moveVelocity * Time.deltaTime);
    }
}