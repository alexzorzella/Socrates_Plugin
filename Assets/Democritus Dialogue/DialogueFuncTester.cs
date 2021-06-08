using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Dialogue_Superclass;

public class DialogueFuncTester : MonoBehaviour
{
    private void Start()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(FuncTester());
    }

    public DialogueSection FuncTester()
    {
        string localName = "Local Name";
        string sound = "dialogue";

        Monologue delay = new Monologue(localName, "This is testing whether the...[delay,1][!delay] delayed text works.", sound);
        Monologue color = new Monologue(localName, "This is testing whether the [color,blue]colorful text[!color] works.", sound, delay);
        Monologue shake = new Monologue(localName, "This is testing whether the [shake,5]shaky text works[!shake].", sound, color);
        Monologue wave = new Monologue(localName, "This is testing whether the [wave]wavy text works[!wave].", sound, shake);
        Monologue basic = new Monologue(localName, "This is testing whether the basic text scroll works.", sound, wave);
        Choices loopQuestion = new Choices(localName, "Now, try again?", sound, ChoiceList(Choice("Yes", basic), Choice("No", null)));
        delay.next = loopQuestion;

        return basic;
    }
}