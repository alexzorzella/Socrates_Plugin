using System.Collections.Generic;
using System;

public sealed class StateMachineState {
    readonly string name;
    float length = float.MaxValue;
    
    readonly Dictionary<StateMachineEvent, StateMachineState> transitions = new();
    readonly Dictionary<Predicate<object>, StateMachineState> entryTransitions = new();
    
    public StateMachineState(string name) {
        this.name = name;
    }
    
    /// <summary>
    /// Adds a transition to another state when a certain StateMachineEvent is handled.
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
    /// Adds a skip transition to another state associated with a predicate that is re-evaluated on entry.
    /// Many logical scenarios can lead to entering a state with a transition out of it that will never be
    /// hit as the event that would have hit it already has occured. These predicates avoid these scenarios
    /// and jump to these respective states.
    /// </summary>
    /// <param name="checkOnEntry"></param>
    /// <param name="transitionsTo"></param>
    public void AddEntryTransition(Predicate<object> checkOnEntry, StateMachineState transitionsTo) {
        entryTransitions.Add(checkOnEntry, transitionsTo);
    }

    public StateMachineState TryEntryState() {
        StateMachineState result = null;
        
        foreach (var entry in entryTransitions) {
            if (entry.Key.Invoke(null)) {
                result = entry.Value;
                break;
            }
        }

        return result;
    }
    
    public string GetName() {
        return name;
    }
    
    public StateMachineState Handle(StateMachineEvent trigger) {
        return transitions.GetValueOrDefault(trigger);
    }

    public bool TransitionsOnAnimationCompleted() {
        return transitions.ContainsKey(StateMachineEvent.ON_ANIMATION_COMPLETED);
    }

    public float GetLength() {
        return length;
    }

    public void SetLength(float clipLength) {
        length = clipLength;
    }

    public override string ToString() {
        return name;
    }
}