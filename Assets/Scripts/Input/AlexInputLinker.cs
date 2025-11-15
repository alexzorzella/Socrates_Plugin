using UnityEngine;
using System.Collections.Generic;

public class AlexInputLinker : InputHandler {
    readonly int port;
    AlexInput input;
    
    public AlexInputLinker(int port) {
        this.port = port;
        InputManager.instance.RegisterInputHandler(this);
    }
    
    public int GetPort() {
        return port;
    }

    public void OnInputChanged(AlexInput newInput) {
        if (input != null && input.GetOverrideFlag()) {
            return;
        }
        
        input = newInput;
    }

    public EventListener GetEventListener() {
        return null;
    }

    public List<EventType> ListensForEvents() {
        return InputUtility.allButtons;
    }

    public bool PersistsThroughLoads() {
        return false;
    }

    public Vector2 GetLeftStick_BucketBrigade() {
        if (input == null) {
            return Vector2.zero;
        }
        
        return input.LeftStick();
    }

    public Vector2 GetLeftStickForgiving_BucketBrigade() {
        if (input == null) {
            return Vector2.zero;
        }
        
        return input.LeftStickForgiving();
    }

    public void SetLeftStickOverride_BucketBrigade(Vector2 setTo) {
        if (input == null) {
            return;
        }
        
        input.SetLeftStickOverride(setTo);
    }

    public void SetOverrideFlag(GameObject bodyObject) {
        if(input == null) {
            input = bodyObject.AddComponent<AlexInput>();
        }
        
        input.SetOverrideFlag();
    }
}