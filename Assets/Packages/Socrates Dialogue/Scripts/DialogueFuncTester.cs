using UnityEngine;
using static Dialogue_Superclass;

public class DialogueFuncTester : MonoBehaviour {
    public void TriggerDialogue() {
        FindObjectOfType<DialogueManager>().StartDialogue(FuncTester());
    }

    public DialogueSection FuncTester() {
        var localName = "Alex";
        var sound = "dialogue";

        var l = new Monologue(localName,
            "This [color,yellow]dialogue system[!color] and [color,yellow]text interpreter[!color] took [wave]years[!wave] of updating to get here, thanks to my dad.",
            sound);
        var delay = new Monologue(localName,
            "This, well, this is supposed to test the delay. Really? I think it worked!", sound, l);
        var color = new Monologue(localName, "This is testing whether the [color,yellow]colorful text[!color] works.",
            sound, delay);
        var shake = new Monologue(localName, "This is testing whether the [shake,7]shaky text works[!shake].", sound,
            color);
        var wave = new Monologue(localName, "This is testing whether the [wave]wavy text works[!wave].", sound, shake);
        var basic = new Monologue(localName, "This is testing whether the basic text scroll works.", sound, wave);
        var loopQuestion = new Choices(localName, "Now, try again?", sound,
            ChoiceList(Choice("Yes", basic), Choice("No", null)));
        l.next = loopQuestion;

        return basic;
    }
}