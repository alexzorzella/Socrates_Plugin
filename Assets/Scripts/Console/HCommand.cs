using System.Collections.Generic;

public interface HCommand {
    string Keyword();
    string CommandHelp();
    List<string> AutocompleteOptions();
    string CommandFunction(params string[] parameters);
}