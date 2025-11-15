using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SocraticVertexFuncTester : MonoBehaviour
{
    public SocraticVertexModifier vertexModifier;
    
    [TextArea(1, 2)]
    public List<string> contents;
    public int currentIndex = -1;

    private void Update()
    {
        // if (Mouse.current.leftButton.wasPressedThisFrame)
        // {
        //     SetContents();
        // }
    }

    public void SetContents()
    {
        // currentIndex++;
        //
        // if (currentIndex > contents.Count)
        // {
        //     currentIndex = 0;
        // }
        //
        // SocraticVertexModifier.PrepareParsesAndSetText(contents[currentIndex], vertexModifier, true, true);
    }
}