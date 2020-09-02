using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Choice
{
    public string message;

    public DialogueMessage nextMessage;

    public Choice(string message)
    {
        this.message = message;
    }
}