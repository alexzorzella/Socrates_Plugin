using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    #region Variables

    [Header("Static")]
    public int health;
    public int maxHealth;
    public float speed;
    public float jumpForce;
    public int extraJumpsValue;

    [Header("Malleable")]
    public float moveInput;
    public Rigidbody2D rb;
    private bool facingRight;

    private int extraJumps;
    private bool isGrounded;
    public Transform groundCheck;
    public Vector2 boxGroundCheck;
    public LayerMask whatIsGround;

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

    #endregion

    private void Start()
    {
        InitializeValues();
    }

    private void InitializeValues()
    {
        PCamera cam = FindObjectOfType<PCamera>();
        cam.SetTargetWithTransform(transform);
        cam.gameObject.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    private void Update()
    {
        Jump();
        OnGrounded();
        CheckForDeathPlane();
    }
    private void CheckForDeathPlane()
    {
        if(transform.position.y < -20)
        {
            Die();
        }
    }

    public void Damage(int damage)
    {
        health -= damage;

        AudioManager.i.Play("scream_short");
        GameAssets.Particle("guts", transform.position);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if(!eligibleToDie)
        {
            return;
        }

        health = 0;

        eligibleToDie = false;

        NATransition.i.anim.SetTrigger("trans");

        rb.bodyType = RigidbodyType2D.Static;

        NATransition.Transition(SceneManager.GetActiveScene().buildIndex);
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
            fPressedJumpRemember = fPressedJumpRememberTime;
        }

        if ((fPressedJumpRemember > 0) && extraJumps > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce);
            fPressedJumpRemember = 0;
            extraJumps--;
        }
        else if ((fPressedJumpRemember > 0) && (extraJumps == 0) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce);
            fPressedJumpRemember = 0;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * fJumpHeightCut);
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

        if (rb.bodyType == RigidbodyType2D.Static)
        {
            return;
        }

        float fHorizontalVelocity = rb.linearVelocity.x - (currentAddedVelocity == Vector2.zero ? 0 : currentAddedVelocity.x);
         
        float moveInput = Input.GetAxisRaw("Horizontal");

        fHorizontalVelocity += (moveInput * speed);

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

        float yVerticalVelocity = rb.linearVelocity.y - (currentAddedVelocity == Vector2.zero ? 0 : currentAddedVelocity.y);

        float finalCurrentAddedY = currentAddedVelocity.y < 0 ? currentAddedVelocity.y * 1.5F : currentAddedVelocity.y;

        rb.linearVelocity = new Vector2(fHorizontalVelocity + currentAddedVelocity.x, yVerticalVelocity + finalCurrentAddedY);

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