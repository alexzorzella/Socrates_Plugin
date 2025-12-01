using System.Collections.Generic;

public class HcCloseConsole : HCommand {
    public List<string> AutocompleteOptions() {
        return new List<string>();
    }

    public string CommandFunction(params string[] parameters) {
        JConsole.i.CloseConsole();
        return "";
    }

    public string CommandHelp() {
        return "Closes the console";
    }

    public string Keyword() {
        return "console";
    }
}