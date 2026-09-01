using System.Collections.Generic;
using SocratesDialogue;

public class HcDialogue : HCommand {
    List<string> options;
    static readonly string testFromScript = "fromScript";
    static readonly string endConversation = ".endConversation";

    public string CommandFunction(params string[] parameters) {
        if (parameters.Length < 2) {
            return "Please specify a dialogue reference ID";
        }

        string sectionReference = parameters[1];

        if (sectionReference == endConversation) {
            if (DialogueManager.i.Talking()) {
                DialogueManager.i.EndDialogue();
                return "Ended conversation";
            }

            return "No current conversation";
        }
        
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
        
        JConsole.i.SetVisible(false);
        JConsole.i.UpdateVisuals();

        string message = $"Starting dialogue at {sectionReference}...";
        
        JConsole.i.DisplaySystemMessage(message);
        
        return message;
    }

    public string Keyword() {
        return "dialogue";
    }

    public string CommandHelp() {
        return "Starts a dialogue with the passed referenceId";
    }

    public List<string> AutocompleteOptions() {
        if (options == null) {
            options = DialogueManifest.GetSectionReferences(true);
            options.Add(testFromScript);
            options.Add(endConversation);
        }

        return options;
    }
}