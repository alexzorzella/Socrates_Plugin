using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchellosAnnotation : MonoBehaviour
{
    [Header("Wave")]
    public bool wave_warpTextVerticies = false;
    public char wave_textWaveAnnotationCharacter = '*';
    public float wave_freqMultiplier = .025F;
    public float wave_amplitude = 7F;
    public float wave_speed = 9F;
}