using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour, MovesAlongPlayer
{
    Rigidbody2D rb;
    public bool on;
    public Transform platform;
    public Transform[] transforms;
    public int currentIndex = 0;
    public float startBufferTime = 2;
    private float currentTime;
    public float speed;
    public Transform[] playerCheckLocations;
    public Vector2 checkSize;
    public LayerMask mask;

    private void Start()
    {
        rb = platform.GetComponent<Rigidbody2D>();

        if (transforms[0] != null)
        {
            platform.position = transforms[0].position;
        }
    }

    private void Update()
    {
        if(on && EligibleToMove())
        {
            Run();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    float currentLerpTime;
    bool teleported;

    private void Run()
    {
        currentLerpTime += Time.deltaTime;

        float x = Mathf.Abs(platform.position.x - transforms[currentIndex].position.x);
        float y = Mathf.Abs(platform.position.y - transforms[currentIndex].position.y);

        if (ScottsUtility.InRange(x, y, 0.05F))
        {
            Vector2 difference = Vector2.zero;

            if (Player() != null)
            {
                difference = Player().transform.position - platform.position;
            }

            platform.position = transforms[currentIndex].position;
            rb.velocity = Vector2.zero;

            if (!teleported)
            {
                if (Player() != null)
                {
                    Player().transform.position = (Vector2)platform.position + difference;
                    Player().GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                }

                teleported = true;
            }

            if (currentTime <= 0)
            {
                currentLerpTime = 0F;
                currentIndex = IncrementWithOverflow.Run(currentIndex, transforms.Length, 1);
                currentTime = startBufferTime;
            }
            else
            {
                currentTime -= Time.deltaTime;
            }
        }
        else
        {
            rb.velocity = (transforms[currentIndex].position - platform.position).normalized * speed;

            if (teleported)
            {
                teleported = false;
            }
        }
    }

    public bool EligibleToMove()
    {
        foreach (var transformItem in playerCheckLocations)
        {
            if (Physics2D.OverlapBox(transformItem.position, checkSize, 360, mask) != null)
            {
                return false;
            }
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var transformItem in playerCheckLocations)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transformItem.position, checkSize);
        }

        Gizmos.DrawWireSphere(platform.position, 0.5F);
    }

    public Rigidbody2D GetRigidbody2D()
    {
        return rb;
    }

    Collider2D Player()
    {
        Collider2D collider = Physics2D.OverlapCircle(platform.position, 0.5F, mask);
        return collider;
    }
}