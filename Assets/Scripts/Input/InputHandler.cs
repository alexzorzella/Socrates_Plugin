using System.Collections.Generic;

public interface InputHandler {
    int GetPort();
    void OnInputChanged(AlexInput newInput);
    EventListener GetEventListener();
    List<EventType> ListensForEvents();
    bool PersistsThroughLoads();
}