using System.Collections.Generic;

public interface JConsoleLogListener {
    void ReceiveBacklog(List<string> backlog);
    void OnSystemMessageLogged(string message);
    void OnWriteToConsole(string message);
}