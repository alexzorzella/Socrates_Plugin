using System;
using SocratesDialogue;
using UnityEngine;

public class DialogueEventTester : MonoBehaviour, DialogueEventListener {
    void Start() {
        DialogueManager.i.RegisterEventListener(this);
    }

    public void OnEvent(string eventTag, string parameters) {
        Debug.Log($"Dialogue Event {eventTag}: {parameters}");
    }
}