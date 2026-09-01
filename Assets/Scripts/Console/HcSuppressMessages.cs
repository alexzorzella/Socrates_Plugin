using System.Collections.Generic;

public class HcSuppressMessages : HCommand {
    List<string> options = null; 

    public string CommandFunction(params string[] parameters) {
        bool suppressSystemMessages = JConsole.i.ToggleSuppressSystemMessages();
        return $"The system will now {(suppressSystemMessages ? "" : "not ")}suppress messages";
    }

    public string CommandHelp() {
        return "Toggles whether system messages appear outside of the console";
    }

    public string Keyword() {
        return "suppressMessages";
    }

    public List<string> AutocompleteOptions() {
        if (options == null) {
            options = new() {
                "true",
                "false"
            };
        }

        return options;
    }
}