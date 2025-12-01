using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachine {
    readonly string name;
    readonly List<StateMachineState> states = new();
    readonly List<StateMachineListener> listeners = new();

    StateMachineState _currentState = null;

    /**
     * Keeps track of LeanTweens currently active so that
     * unnecessary cancellations do not occur on
     * unpredictable state changes
    */
    readonly List<int> leanTweensInProgress = new();

    StateMachine(string name, List<StateMachineState> states) : 
        this(name, states.ToArray()) {
    }

    StateMachine(string name, params StateMachineState[] states) {
        this.name = name;
        
        AddStates(states);
        
        SetState(states[0], states[0]);
    }

    /// <summary>
    /// Registers a listener to the state machine that reports a state change from a state 'from'
    /// to another state 'to'.
    /// </summary>
    /// <param name="listener"></param>
    public void RegisterListener(StateMachineListener listener) {
        listeners.Add(listener);
        listener.OnStateMachineStateChange(null, _currentState);
    }
    
    /// <summary>
    /// Handles a state machine event.
    /// </summary>
    /// <param name="trigger"></param>
    public void Handle(StateMachineEvent trigger) {
        StateMachineState from = _currentState;
        StateMachineState to = _currentState.Handle(trigger);
        
        if (to != null) {
            SetState(from, to);
        }
    }

    /// <summary>
    /// Jumps the current state to a state with the passed name. If the past name
    /// is not a state in the list of states, the function logs a warning.
    /// </summary>
    /// <param name="stateName"></param>
    public void JumpTo(string stateName) {
        StateMachineState from = _currentState;
        StateMachineState to = states.Find(state => state.GetName() == stateName);

        if (to != null) {
            SetState(from, to);
        } else {
            Debug.LogWarning($"Error: {stateName} does not exist");
        }
    }
    
    void AddState(StateMachineState newStateMachineState) {
        states.Add(newStateMachineState);
    }
    
    void AddStates(params StateMachineState[] newStates) {
        foreach (var newState in newStates) {
            AddState(newState);
        }
    }

    void NotifyListeners(StateMachineState from, StateMachineState to) {
        foreach (var listener in listeners) {
            listener.OnStateMachineStateChange(from, to);
        }
    }

    void CancelAllLeanTweensInProgress() {
        foreach (var leanTweenId in leanTweensInProgress) {
            LeanTween.cancel(leanTweenId);
        }
        leanTweensInProgress.Clear();
    }

    void SetState(StateMachineState from, StateMachineState to) {
        CancelAllLeanTweensInProgress();
        
        StateMachineState onEntryResult = to.TryEntryState();

        if (onEntryResult != null) {
            string onEntryName = onEntryResult.GetName();
            // Debug.Log($"OnEntry found: {onEntryName}, calling SetState({onEntryName})");

            _currentState = to;
            NotifyListeners(from, to);

            SetState(to, onEntryResult);
            return;
        }

        _currentState = to; 
        NotifyListeners(from, to);
        
        // Debug.Log($"Set state to {_currentState.GetName()}");
        
        if (_currentState.TransitionsOnAnimationCompleted()) {
            // Debug.Log($"delayedCall to leanTween to occur after {_currentState.GetLength()} seconds");
            var ltDescr = LeanTween.delayedCall(_currentState.GetLength(), () => {
                Handle(StateMachineEvent.ON_ANIMATION_COMPLETED);
            });
            leanTweensInProgress.Add(ltDescr.id);
        }
    }

    public string CurrentStateName() {
        return _currentState.GetName();
    }

    public override string ToString() {
        string result = $"{name}\n";

        foreach (var state in states) {
            result += $"{state}\n\n";
        }

        return result;
    }

    public class ShellBuilder {
        string name = "State Machine Shell";
        List<StateMachineState> states = new();

        public ShellBuilder WithName(string name) {
            this.name = name;
            return this;
        }
        
        public ShellBuilder WithStates(params string[] stateNames) {
            foreach (var stateName in stateNames) {
                states.Add(new StateMachineState(stateName));
            }
            
            return this;
        }

        public StateMachine Build() {
            StateMachine finalStateMachine = new StateMachine(name, states);
            
            return finalStateMachine;
        }
    }
    
    public static class StateMachineUtility {
        public static void ImportLengthsFromRuntimeController(StateMachine stateMachine, RuntimeAnimatorController runtimeController) {
            foreach (var state in stateMachine.states) {
                if (state.TransitionsOnAnimationCompleted()) {
                    AnimationClip clip = Array.Find(runtimeController.animationClips, clip => clip.name == state.GetName());
                    float clipLength = clip != null ? clip.length : float.MaxValue;
                
                    state.SetLength(clipLength);
                }
            }
        }
    }
}