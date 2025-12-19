using SocratesDialogue;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Continues the current conversation whenever the user left clicks.
/// To change the way the user continues the conversation, just call
/// DialogueManager.ContinueConversation() from somewhere else.
/// </summary>
public class DialogueInteraction : MonoBehaviour {
    void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            DialogueManager.i.ContinueConversation();
        }
    }
}