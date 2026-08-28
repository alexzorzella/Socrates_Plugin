using System.Collections.Generic;
    
public class HcDebug : HCommand {
    readonly List<string> options = new();

    public HcDebug() {
        options = DebugView.i.GetObjectNames();
    }

    public string CommandFunction(params string[] parameters) {
        if (parameters.Length < 2) {
            return $"Please specify of the ({options.Count}) object name(s)";
        }

        string result = "";

        for (int i = 1; i < parameters.Length; i++) {
            string objectName = parameters[i];
            bool active = DebugView.i.ToggleDebugTextObject(objectName);

            result += $"{objectName} is now {(active ? "shown" : "hidden")}";

            if (i < parameters.Length - 1) {
                result += "\n";
            }
        }

        return result;
    }

    public string CommandHelp() {
        return "";
    }

    public string Keyword() {
        return "debug";
    }

    public List<string> AutocompleteOptions() {
        return options;
    }
}
