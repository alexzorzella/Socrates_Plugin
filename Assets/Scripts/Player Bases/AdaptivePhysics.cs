using UnityEngine;
using UnityEngine.SceneManagement;

public class AdaptivePhysics : MonoBehaviour {
    [Header("Malleable")] public float moveInput;

    public Rigidbody2D rb;
    public Transform groundCheck;
    public Vector2 boxGroundCheck;
    public LayerMask whatIsGround;

    [Header("Movement Management")] public float speed = 1F;

    [Range(0f, 1f)] public float jumpCut = 0.65F;

    public float dampenMoving = 5F;
    public float dampenStopping = 5F;
    public float dampenTurning = 6F;

    public int extraJumpsValue;
    public float jumpLeniencyTime = 0.2F;
    public float jumpForce = 300F;

    public Vector2 currentAddedVelocity = Vector2.zero;
    Animator anim;

    bool eligibleToDie = true;

    int extraJumps;
    bool facingRight = true;

    float fPressedJumpRemember;
    bool isGrounded;

    bool lastGrounded;

    void Start() {
        InitializeValues();
    }

    void Update() {
        Jump();
        OnGrounded();
        CheckForDeathPlane();
        UpdateAnimatorInfo();
    }

    void FixedUpdate() {
        MoveAndFlip();
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(groundCheck.position, boxGroundCheck);
    }

    void InitializeValues() {
        var cam = FindFirstObjectByType<Cameraperson>();
        cam.SetTargetWithTransform(transform);

        if (GetComponentInChildren<Animator>() != null) anim = GetComponentInChildren<Animator>();

        cam.transform.position = new Vector3(transform.position.x, transform.position.y, -10F);
    }

    void UpdateAnimatorInfo() {
        if (anim == null) return;

        anim.SetBool("grounded", IsGrounded());
        anim.SetFloat("horizontal", Mathf.Abs(rb.linearVelocity.x));
    }

    void CheckForDeathPlane() {
        if (transform.position.y < -20) Die();
    }

    void Die() {
        if (!eligibleToDie) return;

        eligibleToDie = false;

        rb.bodyType = RigidbodyType2D.Static;

        GnaTransition.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnGrounded() {
        if (lastGrounded != isGrounded) {
            if (isGrounded) {
                
            }
        }

        lastGrounded = isGrounded;
    }

    void Jump() {
        if (isGrounded) extraJumps = extraJumpsValue;

        fPressedJumpRemember -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space)) fPressedJumpRemember = jumpLeniencyTime;

        if (fPressedJumpRemember > 0 && extraJumps > 0) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce);
            fPressedJumpRemember = 0;
            extraJumps--;
        }
        else if (fPressedJumpRemember > 0 && extraJumps == 0 && isGrounded) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce);
            fPressedJumpRemember = 0;
        }

        if (Input.GetKeyUp(KeyCode.Space))
            if (rb.linearVelocity.y > 0)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCut);
    }

    public bool IsMoving() {
        if (moveInput != 0) return true;

        return false;
    }

    public bool IsGrounded() {
        return isGrounded;
    }

    void MoveAndFlip() {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, boxGroundCheck, 360, whatIsGround);

        if (rb.bodyType == RigidbodyType2D.Static) return;

        var fHorizontalVelocity =
            rb.linearVelocity.x - (currentAddedVelocity == Vector2.zero ? 0 : currentAddedVelocity.x);

        var moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0) {
            if (rb.linearVelocity.x < speed) {
                if (Mathf.Sign(moveInput) != Mathf.Sign(fHorizontalVelocity))
                    fHorizontalVelocity += speed * dampenTurning * Time.deltaTime;
                else
                    fHorizontalVelocity += speed * dampenMoving * Time.deltaTime;
            }
        }
        else if (moveInput < 0) {
            if (rb.linearVelocity.x > -speed) {
                if (Mathf.Sign(moveInput) != Mathf.Sign(fHorizontalVelocity))
                    fHorizontalVelocity -= speed * dampenTurning * Time.deltaTime;
                else
                    fHorizontalVelocity -= speed * dampenMoving * Time.deltaTime;
            }
        }
        else {
            if (isGrounded) {
                if (fHorizontalVelocity < 0)
                    fHorizontalVelocity += speed * dampenStopping * Time.deltaTime;
                else if (fHorizontalVelocity > 0) fHorizontalVelocity -= speed * dampenStopping * Time.deltaTime;

                if (fHorizontalVelocity > -1F && fHorizontalVelocity < 1F) fHorizontalVelocity = 0;
            }
        }

        var yVerticalVelocity =
            rb.linearVelocity.y - (currentAddedVelocity == Vector2.zero ? 0 : currentAddedVelocity.y);

        var finalCurrentAddedY = currentAddedVelocity.y < 0 ? currentAddedVelocity.y * 1.5F : currentAddedVelocity.y;

        rb.linearVelocity = new Vector2(fHorizontalVelocity + currentAddedVelocity.x,
            yVerticalVelocity + finalCurrentAddedY);

        if (!facingRight && Input.GetAxisRaw("Horizontal") > 0)
            Flip();
        else if (facingRight && Input.GetAxisRaw("Horizontal") < 0) Flip();
    }

    bool ReturnGroundedFromArray(Collider[] gameObjects) {
        if (gameObjects != null || gameObjects.Length > 0) return true;

        return false;
    }

    void Flip() {
        facingRight = !facingRight;
        var scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    int FacingRightToInt() {
        return facingRight ? 1 : -1;
    }
}