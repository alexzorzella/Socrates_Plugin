using System.Collections.Generic;
using System;
using System.Linq;

public sealed class StateMachineState {
    readonly string name;
    float length = float.MaxValue;
    
    readonly Dictionary<StateMachineEvent, StateMachineState> transitions = new();
    readonly Dictionary<Predicate<object>, StateMachineState> entryTransitions = new();

    // TODO: Make this a HashSet
    readonly Dictionary<StateAtt, bool> attributes = new();
    
    // TODO: This should be readonly (make a builder for these states later)
    AssociatedDetector associatedDetector = AssociatedDetector.NONE;
    
    public StateMachineState(string name, params StateAtt[] attributes) {
        this.name = name;
        AddAttributes(attributes);
    }

    public StateMachineState WithAssociatedDetector(AssociatedDetector associatedDetector) {
        this.associatedDetector = associatedDetector;
        return this;
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

    void AddAttributes(params StateAtt[] newAttributes) {
        foreach (var attribute in newAttributes) {
            if (!attributes.ContainsKey(attribute)) {
                attributes.Add(attribute, true);
            }
        }
    }

    public bool HasAttribute(StateAtt checkFor) {
        return attributes.ContainsKey(checkFor) ? attributes[checkFor] : false;
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

    public AssociatedDetector GetAssociatedDetector() {
        return associatedDetector;
    }
    
    public override string ToString() {
        string result = name;

        result += $"\nAttributes:{(attributes.Count == 0 ? "None" : "")}";

        for (int i = 0; i < attributes.Count; i++) {
            result += $"{attributes.ElementAt(i).Key}{(i < attributes.Count - 1 ? "," : "")}";
        }

        result += $"\nComboDetector: {associatedDetector.ToString()}";
        
        return result;
    }
}

public enum StateAtt {
    IGNORE_COMBO_INPUT,
    IGNORE_PHYSICS_INPUT,
    ABLE_TO_BLOCK,
    FLIP_ENABLED,
    STUNNED
}

public enum AssociatedDetector {
    NONE,
    STANDING,
    AIRBORNE,
    CROUCHED,
    STANDING_OR_CROUCHED,
    ANY,
    SENTINEL
}