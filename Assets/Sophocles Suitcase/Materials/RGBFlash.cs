using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RGBFlash : MonoBehaviour
{
    private Material _mat;
    public float glitchTimeMin = 0.05F, glitchTimeMax = 0.15F;
    private float currentGlitchTime;

    public Vector2 maxGlitchRange, minGlitchRange;
    public bool glitchR, glitchG, glitchB;

    private void Start()
    {
        GetComponents();
    }

    private void GetComponents()
    {
        _mat = GetComponent<SpriteRenderer>().material;
    }

    private void Update()
    {
         DecreaseAndCheckGlitchTime();
    }

    private void DecreaseAndCheckGlitchTime()
    {
        currentGlitchTime -= Time.deltaTime;

        if(currentGlitchTime <= 0)
        {
            if(glitchR)
            {
                _mat.SetVector("_roffset", RandomVector(minGlitchRange, maxGlitchRange));
            }

            if (glitchG)
            {
                _mat.SetVector("_goffset", RandomVector(minGlitchRange, maxGlitchRange));
            }

            if (glitchB)
            {
                _mat.SetVector("_boffset", RandomVector(minGlitchRange, maxGlitchRange));
            }

            ResetGlitchTime();
        }
    }

    private void ResetGlitchTime()
    {
        currentGlitchTime = Random.Range(glitchTimeMin, glitchTimeMax);
    }

    private Vector2 RandomVector(Vector2 min, Vector2 max)
    {
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);

        return new Vector2(x, y);
    }
}