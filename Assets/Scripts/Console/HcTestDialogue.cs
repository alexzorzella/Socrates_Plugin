using System.Collections.Generic;
using SocratesDialogue;

public class HcTestDialogue : HCommand {
    List<string> options = null;
    static readonly string testFromScript = "fromScript";

    public string CommandFunction(params string[] parameters) {
        if (parameters.Length < 2) {
            return "Please specify a dialogue to test...";
        }

        string sectionReference = parameters[1];

        if (sectionReference == testFromScript) {
            DialogueManager.i.StartDialogue(new DialogueTest().Dialogue());
        } else {
            DialogueSection section = DialogueManifest.GetSectionByReference(sectionReference);

            if (section != null) {
                DialogueManager.i.StartDialogue(section);
            } else {
                return $"No section called {sectionReference} found";
            }
        }
        
        JConsole.i.visible = false;
        JConsole.i.UpdateVisuals();

        string message = $"Starting dialogue at {sectionReference}...";
        
        JConsole.i.DisplaySystemMessage(message);
        
        return message;
    }

    public string Keyword() {
        return "testDialogue";
    }

    public string CommandHelp() {
        return "Plays test dialogue";
    }

    public List<string> AutocompleteOptions() {
        if (options == null) {
            options = DialogueManifest.GetSectionReferences();
            options.Add(testFromScript);
        }

        return options;
    }
}