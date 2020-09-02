using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcZ
{
    public DialogueMessage Create()
    {
        DialogueMessage messageName = new DialogueMessage(
          "What is your name?",
          "Tom",
          "Jerry");

        DialogueMessage messageColor = new DialogueMessage(
            "What is your favorite color?",
            "blue", "red", "green");

        DialogueMessage messageWeight = new DialogueMessage(
            "What is the weight a sparrow can carry?",
            "1 Lb",
            "2 Lb");

        messageName.Link(messageColor, messageColor);
        messageColor.Link(messageName, messageWeight, ZDialogueManager.TERMINATING);
        messageWeight.Link(messageColor, messageColor);

        return messageName;
    }
}
