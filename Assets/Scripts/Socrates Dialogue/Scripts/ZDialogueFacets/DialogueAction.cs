using System;
using SocratesDialogue;

public class DialogueAction : ZDialogueFacet {
    public enum DialogueActionTime { BEFORE_DISPLAYING_TEXT, AFTER_DISPLAYING_TEXT }

    readonly Action action;
    readonly DialogueActionTime dialogueActionTime;
    
    public DialogueAction(Action action, DialogueActionTime dialogueActionTime = DialogueActionTime.BEFORE_DISPLAYING_TEXT) {
        this.action = action;
        this.dialogueActionTime = dialogueActionTime;
    }

    public void Trigger() {
        action.Invoke();
    }
}