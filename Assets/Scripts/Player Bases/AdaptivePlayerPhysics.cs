using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class AdaptivePlayerPhysics : MonoBehaviour
{
    Animator anim;

    public float speed;
    public int extraJumpsValue;
    public float jumpLeniencyTime;
    public float jumpForce;

    [Header("Malleable")]
    public float moveInput;
    public Rigidbody2D rb;
    private bool facingRight = true;

    private int extraJumps;
    private bool isGrounded;
    public Transform groundCheck;
    public Vector2 boxGroundCheck;
    public LayerMask whatIsGround;

    private float fPressedJumpRemember;

    [Header("Movement Management")]
    [Range(0f, 1f)]
    public float jumpCut = 0.65F;
    public float dampenMoving = 5F;
    public float dampenStopping = 5F;
    public float dampenTurning = 6F;
    
    [Range(0f, 1f)]
    public float fHorizontalDampeningBasic;
    [Range(0f, 1f)]
    public float fHorizontalDampeningWhenStopping;
    [Range(0f, 1f)]
    public float fHorizontalDampeningWhenTurning;

    private void Start()
    {
        InitializeValues();
    }

    private void InitializeValues()
    {
        CameramanTimothy cam = FindObjectOfType<CameramanTimothy>();
        cam.SetTargetWithTransform(transform);
        anim = GetComponentInChildren<Animator>();
        cam.transform.position = new Vector3(transform.position.x, transform.position.y, -10F);
    }

    private void Update()
    {
        Jump();
        OnGrounded();
        CheckForDeathPlane();
        UpdateAnimatorInfo();
    }

    private void UpdateAnimatorInfo()
    {
        anim.SetBool("grounded", IsGrounded());
        anim.SetFloat("horizontal", Mathf.Abs(rb.velocity.x));
    }

    private void CheckForDeathPlane()
    {
        if (transform.position.y < -20)
        {
            Die();
        }
    }

    private void Die()
    {
        if (!eligibleToDie)
        {
            return;
        }

        eligibleToDie = false;

        TimothysTransition.i.anim.SetTrigger("trans");

        rb.bodyType = RigidbodyType2D.Static;

        TimothysTransition.Transition(SceneManager.GetActiveScene().buildIndex);
    }

    bool eligibleToDie = true;

    bool lastGrounded;

    private void OnGrounded()
    {
        if (lastGrounded != isGrounded)
        {
            if (isGrounded)
            {

            }
        }

        lastGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        MoveAndFlip();
    }

    private void Jump()
    {
        if (isGrounded)
        {
            extraJumps = extraJumpsValue;
        }

        fPressedJumpRemember -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            fPressedJumpRemember = jumpLeniencyTime;
        }

        if ((fPressedJumpRemember > 0) && extraJumps > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce);
            fPressedJumpRemember = 0;
            extraJumps--;
        }
        else if ((fPressedJumpRemember > 0) && (extraJumps == 0) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce);
            fPressedJumpRemember = 0;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCut);
            }
        }
    }

    public bool IsMoving()
    {
        if (moveInput != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    private void MoveAndFlip()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, boxGroundCheck, 360, whatIsGround);

        if (isGrounded)
        {
            MovesAlongPlayer platform = Physics2D.OverlapBox(groundCheck.position, boxGroundCheck, 360, whatIsGround).gameObject.GetComponentInParent<MovesAlongPlayer>();

            if (platform != null)
            {
                currentAddedVelocity = platform.GetRigidbody2D().velocity;
            }
            else
            {
                currentAddedVelocity = Vector2.zero;
            }
        }

        if (rb.bodyType == RigidbodyType2D.Static)
        {
            return;
        }

        float fHorizontalVelocity = rb.velocity.x - (currentAddedVelocity == Vector2.zero ? 0 : currentAddedVelocity.x);

        float moveInput = Input.GetAxisRaw("Horizontal");

        if (FindObjectOfType<DialogueManager>().Talking())
        {
            moveInput = 0;
        }

        if (moveInput > 0)
        {
            if (rb.velocity.x < speed)
            {
                if (Mathf.Sign(moveInput) != Mathf.Sign(fHorizontalVelocity))
                {
                    fHorizontalVelocity += speed * dampenTurning * Time.deltaTime;
                }
                else
                {
                    fHorizontalVelocity += speed * dampenMoving * Time.deltaTime;
                }
            }
        }
        else if (moveInput < 0)
        {
            if (rb.velocity.x > -speed)
            {
                if (Mathf.Sign(moveInput) != Mathf.Sign(fHorizontalVelocity))
                {
                    fHorizontalVelocity -= speed * dampenTurning * Time.deltaTime;
                }
                else
                {
                    fHorizontalVelocity -= speed * dampenMoving * Time.deltaTime;
                }
            }
        }
        else
        {
            if (isGrounded)
            {
                if (fHorizontalVelocity < 0)
                {
                    fHorizontalVelocity += speed * dampenStopping * Time.deltaTime;
                }
                else if (fHorizontalVelocity > 0)
                {
                    fHorizontalVelocity -= speed * dampenStopping * Time.deltaTime;
                }

                if (fHorizontalVelocity > -1F && fHorizontalVelocity < 1F)
                {
                    fHorizontalVelocity = 0;
                }
            }
        }

        float yVerticalVelocity = rb.velocity.y - (currentAddedVelocity == Vector2.zero ? 0 : currentAddedVelocity.y);

        float finalCurrentAddedY = currentAddedVelocity.y < 0 ? currentAddedVelocity.y * 1.5F : currentAddedVelocity.y;

        rb.velocity = new Vector2(fHorizontalVelocity + currentAddedVelocity.x, yVerticalVelocity + finalCurrentAddedY);

        if (facingRight == false && Input.GetAxisRaw("Horizontal") > 0)
        {
            Flip();
        }
        else if (facingRight == true && Input.GetAxisRaw("Horizontal") < 0)
        {
            Flip();
        }
    }

    private bool ReturnGroundedFromArray(Collider[] gameObjects)
    {
        if (gameObjects != null || gameObjects.Length > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    public Vector2 currentAddedVelocity = Vector2.zero;

    private int FacingRightToInt()
    {
        return facingRight ? 1 : -1;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(groundCheck.position, boxGroundCheck);
    }
}