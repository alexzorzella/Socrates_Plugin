using System;
using SocratesDialogue;

public enum DialogueActionTime { BEFORE_DISPLAYING_TEXT, AFTER_DISPLAYING_TEXT }

public class DialogueAction : ZDialogueFacet {

    readonly Action action;
    readonly DialogueActionTime dialogueActionTime;
    
    public DialogueAction(Action action, DialogueActionTime dialogueActionTime = DialogueActionTime.BEFORE_DISPLAYING_TEXT) {
        this.action = action;
        this.dialogueActionTime = dialogueActionTime;
    }

    public DialogueActionTime GetDialogueActionTime() {
        return dialogueActionTime;
    }

    public void Invoke() {
        action.Invoke();
    }
}