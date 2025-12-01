using System.Collections.Generic;

public class HcClearConsole : HCommand {
    public List<string> AutocompleteOptions() {
        return new List<string>();
    }

    public string CommandFunction(params string[] parameters) {
        JConsole.i.ClearConsole();
        return "";
    }

    public string CommandHelp() {
        return "Clears the console";
    }

    public string Keyword() {
        return "clear";
    }
}