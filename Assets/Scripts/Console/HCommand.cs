using System.Collections.Generic;

public interface HCommand {
    string CommandFunction(params string[] parameters);
    string CommandHelp();
    string Keyword();
    List<string> AutocompleteOptions();
}