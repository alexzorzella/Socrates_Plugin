using System;
using SocratesDialogue;
using UnityEngine;

/// <summary>
/// This test script registers to all dialogue events.
/// </summary>
public class DialogueEventTester : MonoBehaviour, DialogueEventListener {
    void Start() {
        DialogueManager.i.RegisterEventListener(this);
    }

    /// <summary>
    /// Whenever any dialogue event occurs, its tag and parameters are logged in the console.
    /// </summary>
    /// <param name="eventTag"></param>
    /// <param name="parameters"></param>
    public void OnEvent(string eventTag, string parameters) {
        Debug.Log($"Dialogue Event {eventTag}: {parameters}");
    }
}