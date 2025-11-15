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

    public bool Currently(StateAtt checkFor) {
        return _currentState.HasAttribute(checkFor);
    }

    public AssociatedDetector CurrentlyAssociatedDetector() {
        return _currentState.GetAssociatedDetector();
    }

    public override string ToString() {
        string result = $"{name}\n";

        foreach (var state in states) {
            result += $"{state}\n\n";
        }

        return result;
    }
    
    public class Builder {
        // These are readonly as they cannot be set after the initial constructor
        readonly StateMachineState _idle;
        readonly StateMachineState _crouch; 
        readonly StateMachineState _jump;
        readonly StateMachineState _land;

        public StateMachineState Idle() {
            return _idle;
        }

        public StateMachineState Crouch() {
            return _crouch;
        }

        public StateMachineState Jump() {
            return _jump;
        }

        public StateMachineState Land() {
            return _land;
        }
        
        readonly List<StateMachineState> _states = new();

        public Builder(AlexInputLinker inputLinker) {
            _idle = new StateMachineState("idle", StateAtt.ABLE_TO_BLOCK, StateAtt.FLIP_ENABLED).WithAssociatedDetector(
                AssociatedDetector.STANDING);
            
            StateMachineState walkForward =
                new StateMachineState("walk_forward", StateAtt.ABLE_TO_BLOCK, StateAtt.FLIP_ENABLED)
                    .WithAssociatedDetector(AssociatedDetector.STANDING);
            StateMachineState walkBackward =
                new StateMachineState("walk_backward", StateAtt.ABLE_TO_BLOCK, StateAtt.FLIP_ENABLED)
                    .WithAssociatedDetector(AssociatedDetector.STANDING);

            _jump = new StateMachineState("jump", StateAtt.ABLE_TO_BLOCK, StateAtt.FLIP_ENABLED).WithAssociatedDetector(
                AssociatedDetector.AIRBORNE);
            _land = new StateMachineState("land", StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT,
                StateAtt.FLIP_ENABLED);

            _crouch = new StateMachineState("crouch", StateAtt.IGNORE_PHYSICS_INPUT, StateAtt.ABLE_TO_BLOCK)
                .WithAssociatedDetector(AssociatedDetector.CROUCHED);

            StateMachineState block = new StateMachineState("block",
                StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT);
            block.AddTransition(StateMachineEvent.ON_ANIMATION_COMPLETED, _idle);
            
            StateMachineState crouchBlock = new StateMachineState("crouch_block",
                StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT);
            crouchBlock.AddTransition(StateMachineEvent.ON_ANIMATION_COMPLETED, _crouch);
            
            StateMachineState stunned = new StateMachineState("stunned", 
                StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT,
                StateAtt.STUNNED);
            // stunned.AddTransition(StateMachineEvent.ON_ANIMATION_COMPLETED, _idle);
            
            StateMachineState intro = new StateMachineState("intro", 
                StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT);
            intro.AddTransition(StateMachineEvent.ON_ANIMATION_COMPLETED, _idle);

            StateMachineState knockout = new StateMachineState("knockout",
                StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT);
            
            StateMachineState win = new StateMachineState("win",
                StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT);
            
            _idle.AddTransition(StateMachineEvent.WALK_FORWARD, walkForward);
            _idle.AddTransition(StateMachineEvent.WALK_BACKWARD, walkBackward);
            _idle.AddTransition(StateMachineEvent.JUMP, _jump);
            _idle.AddTransition(StateMachineEvent.CROUCH, _crouch);
            _idle.AddEntryTransition((object o) => (inputLinker.GetLeftStickForgiving_BucketBrigade().x > 0), walkForward);
            _idle.AddEntryTransition((object o) => (inputLinker.GetLeftStickForgiving_BucketBrigade().x < 0), walkBackward);
            _idle.AddEntryTransition((object o) => (inputLinker.GetLeftStickForgiving_BucketBrigade().y < 0), _crouch);

            walkForward.AddTransition(StateMachineEvent.STOP, _idle);
            walkForward.AddTransition(StateMachineEvent.WALK_BACKWARD, walkBackward);
            walkForward.AddTransition(StateMachineEvent.JUMP, _jump);
            walkForward.AddTransition(StateMachineEvent.CROUCH, _crouch);

            walkBackward.AddTransition(StateMachineEvent.STOP, _idle);
            walkBackward.AddTransition(StateMachineEvent.WALK_FORWARD, walkForward);
            walkBackward.AddTransition(StateMachineEvent.JUMP, _jump);
            walkBackward.AddTransition(StateMachineEvent.CROUCH, _crouch);

            _jump.AddTransition(StateMachineEvent.LAND, _land);
            _land.AddTransition(StateMachineEvent.ON_ANIMATION_COMPLETED, _idle);

            _crouch.AddTransition(StateMachineEvent.UN_CROUCH, _idle);
            _crouch.AddEntryTransition((object o) => (inputLinker.GetLeftStickForgiving_BucketBrigade().y >= 0), _idle);
            
            _states = new() {
                _idle,
                intro,
                walkForward,
                walkBackward,
                _jump,
                _land,
                _crouch,
                block,
                crouchBlock,
                stunned,
                knockout,
                win
            };
            
            foreach (string damageStateName in Damage.uniqueDamageNames) {
                StateMachineState damageState = new StateMachineState(damageStateName, 
                    StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT);

                if (damageStateName.Contains("_air")) {
                    damageState.AddTransition(StateMachineEvent.LAND, _land);
                    // This is where you can make them transition to a prone -> get_up state
                    
                    if (damageStateName.Contains("_ascending")) {
                        string descendingName = damageStateName.Replace("_ascending", "_descending");
                        
                        StateMachineState fallingState = new StateMachineState(descendingName, 
                            StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT);
                        damageState.AddTransition(StateMachineEvent.FALL, fallingState);
                        
                        fallingState.AddTransition(StateMachineEvent.LAND, _land);
                        _states.Add(fallingState);
                    }
                } else if(!damageStateName.Contains("_grabbed")) {
                    damageState.AddTransition(StateMachineEvent.ON_ANIMATION_COMPLETED, _idle);
                }
                
                _states.Add(damageState);
            }
            
            AddMoveBatch(MoveUtil.genericMoves);
        }
        
        public Builder AddMoveBatch(List<Move> moves) {
            // Add all generic and character-specific move states
            foreach (Move move in moves) {
                StateMachineState moveState = null;

                if (move.ExitsOnEvents().Contains(StateMachineEvent.JUMP)) {
                    moveState = new StateMachineState(move.GetCombo().GetName(), StateAtt.IGNORE_COMBO_INPUT);
                } else {
                    moveState = new StateMachineState(move.GetCombo().GetName(), StateAtt.IGNORE_COMBO_INPUT, StateAtt.IGNORE_PHYSICS_INPUT);
                }

                StateMachineState returnToOnCompletion = null;  

                switch (move.GetCombo().GetAssociatedDetector()) {
                    case AssociatedDetector.AIRBORNE:
                        returnToOnCompletion = _jump;
                        moveState.AddTransition(StateMachineEvent.LAND, _land);
                        break;
                    case AssociatedDetector.STANDING:
                        returnToOnCompletion = _idle;
                        break;
                    case AssociatedDetector.CROUCHED:
                        returnToOnCompletion = _crouch;
                        break;
                    case AssociatedDetector.STANDING_OR_CROUCHED:
                        returnToOnCompletion = _idle;
                        break;
                    case AssociatedDetector.ANY:
                        returnToOnCompletion = _idle;
                        break;
                    case AssociatedDetector.NONE:
                        break;
                    case AssociatedDetector.SENTINEL:
                        returnToOnCompletion = _idle;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown associated detector: " +
                                                            $"{move.GetCombo().GetAssociatedDetector()}");
                }

                List<StateMachineEvent> exitEvents = move.ExitsOnEvents();

                foreach (StateMachineEvent exitEvent in exitEvents) {
                    moveState.AddTransition(exitEvent, returnToOnCompletion);
                }
            
                _states.Add(moveState);
            }

            return this;
        }
        
        public Builder WithSpecialMoves() {
            return this;
        }

        Builder AddState(StateMachineState state) {
            _states.Add(state);
            
            return this;
        }
        
        public void AddStates(params StateMachineState[] states) {
            foreach (var state in states) {
                AddState(state);
            }
        }
        
        public StateMachine Build(CharacterAnimator characterAnimator) {
            StateMachine finalStateMachine = new StateMachine("State Machine", _states);
            StateMachineUtility.ImportLengthsFromRuntimeController(finalStateMachine, characterAnimator.GetRuntimeAnimatorController());
            
            finalStateMachine.RegisterListener(characterAnimator);
            
            return finalStateMachine;
        }
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

        public StateMachine Build(CharacterAnimator characterAnimator) {
            StateMachine finalStateMachine = new StateMachine(name, states);
            StateMachineUtility.ImportLengthsFromRuntimeController(finalStateMachine, characterAnimator.GetRuntimeAnimatorController());
            
            finalStateMachine.RegisterListener(characterAnimator);
            
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