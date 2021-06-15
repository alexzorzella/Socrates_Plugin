using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CatLogo : MonoBehaviour
{
    public string content;
    public SocraticVertexModifier vertexModifier;
    Animator anim;

    private void Start()
    {
        SetText();
    }

    private void SetText()
    {
        anim = GetComponent<Animator>();
        SocraticVertexModifier.PrepareParsesAndSetText(content, vertexModifier, false, false);
    }

    private void Update()
    {
        UpdateCat();
    }

    private void UpdateCat()
    {
        anim.SetBool("talking", !vertexModifier.TextHasBeenDisplayed());
    }
}