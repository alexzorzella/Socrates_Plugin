using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachine {
    readonly string name;
    readonly List<StateMachineState> states = new();
    readonly List<StateMachineListener> listeners = new();

    StateMachineState currentState;

    float speed;
    
    // Keeps track of LeanTweens currently active so that
    // unnecessary cancellations do not occur on
    // unpredictable state changes
    readonly List<int> leanTweensInProgress = new();

    const int maximumSetStateDepth = 100;
    
    StateMachine(string name, params StateMachineState[] states) {
        this.name = name;
        AddStates(states);

        if (this.states.Count > 0) {
            SetState(states[0], states[0]);
        }
    }
    
    StateMachine(string name, List<StateMachineState> states) : this(name, states.ToArray()) { }

    /// <summary>
    /// Registers a listener to the state machine that reports a state change from a state 'from'
    /// to another state 'to'.
    /// </summary>
    /// <param name="listener"></param>
    public void RegisterListener(StateMachineListener listener) {
        listeners.Add(listener);
        listener.OnStateMachineStateChange(null, currentState);
    }
    
    /// <summary>
    /// Notifies all registered listeners of a state change from StateMachineState from to
    /// StateMachineState to
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    void NotifyListeners(StateMachineState from, StateMachineState to) {
        foreach (var listener in listeners) {
            listener.OnStateMachineStateChange(from, to);
        }
    }
    
    /// <summary>
    /// Handles a state machine event by checking if the current state
    /// has a transition for the passed StateMachineEvent
    /// </summary>
    /// <param name="trigger"></param>
    public void Handle(StateMachineEvent trigger) {
        StateMachineState from = currentState;
        StateMachineState to = currentState.Handle(trigger);
        
        if (to != null) {
            SetState(from, to);
        }
    }

    /// <summary>
    /// Sets the current state to StateMachineState to and notifies the subscribed listeners
    /// of the state change. Entry transitions are handled recursively. To safeguard against
    /// infinite transitions, the recursive call stack is limited to a maximum recursive depth.
    /// 
    /// If the final current state transitions when its animation is completed, a delayed
    /// LeanTween call is made to handle an ON_ANIMATION_COMPLETED event after the animation's
    /// runtime.
    ///
    /// Note: for transitions ON_ANIMATION_COMPLETED to work, the state machine's state times
    /// must be populated. Use StateMachine.Utility.ImportLengthsFromRuntimeController to
    /// automatically import times. Additionally, speed must reflect the animator's speed.
    /// If the animator's speed is changed, the speed must be updated here.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="depth"></param>
    void SetState(StateMachineState from, StateMachineState to, int depth = 0) {
        CancelAllLeanTweensInProgress();
        
        StateMachineState onEntryResult = to.TryEntryTransitions();
        currentState = to;
        NotifyListeners(from, to);

        if (onEntryResult != null && depth < maximumSetStateDepth) {
            SetState(to, onEntryResult, depth + 1 );
            return;
        }
        
        if (currentState.TransitionsOnAnimationCompleted()) {
            var ltDescr = LeanTween.delayedCall(currentState.GetLength() / speed, () => {
                Handle(StateMachineEvent.ON_ANIMATION_COMPLETED);
            });
            
            leanTweensInProgress.Add(ltDescr.id);
        }
    }

    /// <summary>
    /// Jumps from the current state to a state with the passed name. If the passed name
    /// is not a state in the list of states, nothing happens.
    /// </summary>
    /// <param name="stateName"></param>
    public void JumpTo(string stateName) {
        StateMachineState from = currentState;
        StateMachineState to = states.Find(state => state.GetName() == stateName);

        if (to != null) {
            SetState(from, to);
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

    /// <summary>
    /// Sets the speed that will be used for transitions ON_ANIMATION_COMPLETED.
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed) { this.speed = speed; } 
   
    /// <summary>
    /// Returns the current state's name.
    /// </summary>
    /// <returns></returns>
    public string GetCurrentStateName() { return currentState.GetName(); }

    void CancelAllLeanTweensInProgress() {
        foreach (var leanTweenId in leanTweensInProgress) {
            LeanTween.cancel(leanTweenId);
        }
        leanTweensInProgress.Clear();
    }

    public override string ToString() {
        string result = $"{name}\n";

        foreach (var state in states) {
            result += $"{state}\n\n";
        }

        return result;
    }

    public class Builder {
        readonly string name;
        readonly List<StateMachineState> states = new();

        public Builder(string name) {
            this.name = name;
        }
        
        /// <summary>
        /// Adds the passed state to the list of states.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public Builder WithState(StateMachineState state) {
            states.Add(state);
            
            return this;
        }
    
        /// <summary>
        /// Adds the passed list of states to the list of states
        /// one at a time via WithState.
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        public Builder WithStates(List<StateMachineState> states) {
            foreach(var state in states) { WithState(state); }
            return this;
        }

        /// <summary>
        /// Adds the passed list of states to the list of states
        /// one at a time by passing states.ToList() into WithStates.
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        public Builder WithStates(params StateMachineState[] states) {
            WithStates(states.ToList());
            return this;
        }
       
        /// <summary>
        /// Returns a StateMachine with the cached name and states.
        /// </summary>
        /// <returns></returns>
        public StateMachine Build() {
            StateMachine finalStateMachine = new StateMachine(name, states);
            return finalStateMachine;
        }
    }
    
    public class ShellBuilder {
        readonly string name; 
        readonly List<StateMachineState> states = new();
        
        public ShellBuilder(string name) {
            this.name = name;
        }

        /// <summary>
        /// Adds a transitionless state with each of the passed names with no length.
        /// </summary>
        /// <param name="stateNames"></param>
        /// <returns></returns>
        public ShellBuilder WithStates(params string[] stateNames) {
            foreach (var stateName in stateNames) {
                states.Add(new StateMachineState(stateName));
            }
            
            return this;
        }
        
        /// <summary>
        /// Returns a StateMachine with the cached name and states.
        /// </summary>
        public StateMachine Build() {
            StateMachine finalStateMachine = new StateMachine(name, states);
            return finalStateMachine;
        }
    }
    
    public static class Utility {
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
    
    public StateMachineState CurrentState() {
        return currentState;
    }
}
