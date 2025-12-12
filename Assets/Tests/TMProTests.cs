using System;
using TMPro;
using UnityEngine;

public class TMProTests : MonoBehaviour {
    TextMeshProUGUI textComponent;
    
    void Start() {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D)) {
            Debug.Log($"Text: '{textComponent.text}', " +
                      $"textInfo.characterCount: {textComponent.textInfo.characterCount}");
        }
    }
}