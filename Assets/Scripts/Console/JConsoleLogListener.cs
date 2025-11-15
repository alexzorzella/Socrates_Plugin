using System.Collections.Generic;

public interface JConsoleLogListener {
    void RecieveBacklog(List<string> backlog);
    void OnSystemMessageLogged(string message);
}