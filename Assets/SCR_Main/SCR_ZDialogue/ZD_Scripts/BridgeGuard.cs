using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeGuard
{
    public DialogueMessage Create()
    {
        DialogueMessage genderQuestion = new DialogueMessage(
          "#name.",
          "...");

        DialogueMessage messageColor = new DialogueMessage(
            "#name, wake up!",
            "...");

        DialogueMessage messageWeight = new DialogueMessage(
            "What are you doing slacking on the job?",
            "I was just napping.",
            "Wouldn't you like to know.",
            "I dozed off...");

        DialogueMessage messageScold = new DialogueMessage(
            "Bruh you better get to work.",
            "No.",
            "If I had an inscentive...?",
            "What am I even supposed to do?");

        DialogueMessage fired = new DialogueMessage(
           "You're fired.",
           "No.",
           "Your loss.",
           "OK.");

        genderQuestion.Link(messageColor);
        messageColor.Link(messageWeight);
        messageWeight.Link(messageScold, messageScold, messageScold);
        messageScold.Link(fired, fired, fired);
        fired.Link(ZDialogueManager.TERMINATING, ZDialogueManager.TERMINATING, ZDialogueManager.TERMINATING);

        return genderQuestion;

    }



}
