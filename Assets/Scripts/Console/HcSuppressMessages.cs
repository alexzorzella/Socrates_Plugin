using System.Collections.Generic;

public class HcSuppressMessages : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        JConsole.i.suppressSystemMessages = !JConsole.i.suppressSystemMessages;

        return "The system will now " + (JConsole.i.suppressSystemMessages ? "" : "not") + " suppress messages";
    }

    public string CommandHelp() {
        return "Toggles whether system messages appear outside of the console";
    }

    public string Keyword() {
        return "suppressMessages";
    }

    public List<string> AutocompleteOptions() {
        options.Add("true");
        options.Add("false");

        return options;
    }
}