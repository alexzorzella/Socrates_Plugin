using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Dialogue_Superclass;

public class DialogueFuncTester : MonoBehaviour
{
    private void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(FuncTester());
    }

    public DialogueSection FuncTester()
    {
        string localName = "Local Name";
        string sound = "dialogue";

        Monologue delay = new Monologue(localName, "This, well, this is supposed to test the delay. Really? I think it worked!", sound);
        Monologue color = new Monologue(localName, "This is testing whether the [color,yellow]colorful text[!color] works.", sound, delay);
        Monologue shake = new Monologue(localName, "This is testing whether the [shake,7]shaky text works[!shake].", sound, color);
        Monologue wave = new Monologue(localName, "This is testing whether the [wave]wavy text works[!wave].", sound, shake);
        Monologue basic = new Monologue(localName, "This is testing whether the basic text scroll works.", sound, wave);
        Choices loopQuestion = new Choices(localName, "Now, try again?", sound, ChoiceList(Choice("Yes", basic), Choice("No", null)));
        delay.next = loopQuestion;

        return basic;
    }
}