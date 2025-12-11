using System;
using System.Collections.Generic;
using SocratesDialogue;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueInteraction : MonoBehaviour {
    DialogueManager dialogueManager;

    void Start() {
        dialogueManager = GetComponent<DialogueManager>();
    }

    void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            dialogueManager.ContinueConversation();
        }
    }
}