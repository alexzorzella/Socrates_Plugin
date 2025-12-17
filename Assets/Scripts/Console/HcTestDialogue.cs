using System.Collections.Generic;
using SocratesDialogue;

public class HcTestDialogue : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        DialogueManager.i.StartDialogue(DialogueManifest.GetSectionByReference("0"));
        JConsole.i.visible = false;
        JConsole.i.UpdateVisuals();

        return "Testing dialogue...";
    }

    public string Keyword() {
        return "testDialogue";
    }

    public string CommandHelp() {
        return "Plays test dialogue";
    }

    public List<string> AutocompleteOptions() {
        return options;
    }
}