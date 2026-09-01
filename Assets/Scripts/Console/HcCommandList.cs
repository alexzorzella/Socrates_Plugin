using System.Collections.Generic;

public class HcCommandList : HCommand {
    readonly List<string> options = new();

    public string CommandFunction(params string[] parameters) {
        List<HCommand> commands = JConsole.i.GetCommands();
            
        foreach (var command in commands) {
            JConsole.i.WriteLine($"{command.Keyword()} {command.CommandHelp()}");
        }

        return $"Listed {commands.Count} commands.";
    }

    public string CommandHelp() {
        return "Lists all commands and their parameters";
    }

    public string Keyword() {
        return "help";
    }

    public List<string> AutocompleteOptions() {
        return options;
    }
}