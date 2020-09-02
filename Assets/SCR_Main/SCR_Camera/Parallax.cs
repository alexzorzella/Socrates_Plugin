using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Parallax : MonoBehaviour
{
    public float length, startPos;
    public GameObject camera;
    public float parallaxEffect;

    public bool fixedY;
    private float startY;

    private void Start()
    {
        startPos = transform.position.x;

        if(GetComponent<SpriteRenderer>() != null)
        {
            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }
    }

    private void Update()
    {
        float temp = camera.transform.position.x * (1 - parallaxEffect);
        float distance = camera.transform.position.x * parallaxEffect;
        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        if(temp > startPos + length)
        {
            startPos += length;
        } else if(temp < startPos - length)
        {
            startPos -= length;
        }
    }
}