using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZUltra_PC : MonoBehaviour
{
    [Header("Animation")]
    public Animator spriteAnim;

    [Header("Horizontal Movement")]
    public float speed;
    public float jumpForce;
    [HideInInspector]
    public float moveInput;
    public Rigidbody2D rb;
    private bool facingRight;

    [Header("Jumping")]
    public int extraJumpsValue;
    private int extraJumps;
    private bool isGrounded;
    public Transform groundCheck;
    public Vector2 boxGroundCheck;
    private LayerMask whatIsGround;

    private float fPressedJumpRemember;
    private float fPressedJumpRememberTime = 0.2F;

    [Header("Movement Management")]
    [Range(0f, 1f)]
    public float fJumpHeightCut;

    [Range(0f, 1f)]
    public float fHorizontalDampeningBasic;
    [Range(0f, 1f)]
    public float fHorizontalDampeningWhenStopping;
    [Range(0f, 1f)]
    public float fHorizontalDampeningWhenTurning;

    private void Update()
    {
        Jump();
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
            fPressedJumpRemember = fPressedJumpRememberTime;
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
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * fJumpHeightCut);
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

        float fHorizontalVelocity = rb.velocity.x;
        float moveInput = Input.GetAxisRaw("Horizontal");

        fHorizontalVelocity += moveInput * speed;

        if (Mathf.Abs(moveInput) < 0.01F)
        {
            fHorizontalVelocity *= Mathf.Pow(1f - fHorizontalDampeningWhenStopping, Time.deltaTime * 10f);
        }
        else if (Mathf.Sign(moveInput) != Mathf.Sign(fHorizontalVelocity))
        {
            fHorizontalVelocity *= Mathf.Pow(1f - fHorizontalDampeningWhenTurning, Time.deltaTime * 10f);
        }
        else
        {
            fHorizontalVelocity *= Mathf.Pow(1f - fHorizontalDampeningBasic, Time.deltaTime * 10f);
        }

        if (spriteAnim.GetBool("crouching") == true)
        {
            fHorizontalVelocity = 0;
        }

        rb.velocity = new Vector2(fHorizontalVelocity, rb.velocity.y);

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
}