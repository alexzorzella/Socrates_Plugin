using System.Collections.Generic;
using System;
using System.Linq;

public enum StateAttribute {
    
}

public sealed class StateMachineState {
    readonly string name;
    float length = float.MaxValue;
    
    readonly Dictionary<StateMachineEvent, StateMachineState> transitions = new();
    readonly Dictionary<Predicate<object>, StateMachineState> entryTransitions = new();
    readonly HashSet<StateAttribute> attributes = new();
    
    public StateMachineState(string name, params StateAttribute[] attributes) {
        this.name = name;
        
        foreach (var attribute in attributes) {
            if (!this.attributes.Contains(attribute)) {
                this.attributes.Add(attribute);
            }
        }
    }
    
    /// <summary>
    /// Adds a transition to another state on handling the passed StateMachineEvent trigger. Throws an
    /// InvalidOperationException if transition map already contains the passed trigger.
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="transitionsTo"></param>
    public void AddTransition(StateMachineEvent trigger, StateMachineState transitionsTo) {
        if (!transitions.ContainsKey(trigger)) {
            transitions.Add(trigger, transitionsTo);
        } else {
            throw new InvalidOperationException($"The transition triggered by {trigger} you're trying to add already exists in State Machine: '{name}'.");
        }
    }

    /// <summary>
    /// Adds a transition to another state associated with a predicate that is evaluated on entry.
    /// Many logical scenarios can lead to entering a state with a transition out of it that will never be
    /// hit as the event that would have hit it already has occurred.
    /// </summary>
    /// <param name="checkOnEntry"></param>
    /// <param name="transitionsTo"></param>
    public void AddEntryTransition(Predicate<object> checkOnEntry, StateMachineState transitionsTo) {
        entryTransitions.Add(checkOnEntry, transitionsTo);
    }

    /// <summary>
    /// Invokes the predicate associated with each entry transition and returns the first one that succeeds or null
    /// by default.
    /// </summary>
    /// <returns></returns>
    public StateMachineState TryEntryTransitions() {
        foreach (var entry in entryTransitions) {
            if (entry.Key.Invoke(null)) {
                return entry.Value;
            }
        }

        return null;
    }
    
    public string GetName() { return name; }
    
    public float GetLength() { return length; }
    public void SetLength(float clipLength) { length = clipLength; }
    
    /// <summary>
    /// Returns whether the state machine has the passed attribute.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public bool HasAttribute(StateAttribute attribute) {
        return attributes.Contains(attribute);
    }
    
    /// <summary>
    /// Returns the StateMachineState that this state transitions to on the passed trigger or null by default.
    /// </summary>
    /// <param name="trigger"></param>
    /// <returns></returns>
    public StateMachineState Handle(StateMachineEvent trigger) {
        return transitions.GetValueOrDefault(trigger);
    }

    /// <summary>
    /// Returns whether this state has a transition ON_ANIMATION_COMPLETED.
    /// </summary>
    /// <returns></returns>
    public bool TransitionsOnAnimationCompleted() {
        return transitions.ContainsKey(StateMachineEvent.ON_ANIMATION_COMPLETED);
    }

    public override string ToString() {
        string result = name;

        result += $"\tAttributes:{(attributes.Count == 0 ? "None" : "")}";

        foreach (var attribute in attributes) {
            result += $"{attribute}{(attribute == attributes.Last() ? "," : "")}";
        }
        
        return result;
    }
}