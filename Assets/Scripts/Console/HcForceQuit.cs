using System.Collections.Generic;
using UnityEngine;

public class HcForceQuit : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        Application.Quit();

        return "Quitting...";
    }

    public string CommandHelp() {
        return "Force quits the program";
    }

    public string Keyword() {
        return "quit";
    }

    public List<string> AutocompleteOptions() {
        return options;
    }
}