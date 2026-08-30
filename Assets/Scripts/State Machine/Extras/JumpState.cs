using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public enum JumpStateType {
    NONE,
    ASCEND_KEY_DOWN,
    ASCEND_KEY_UP,
    DESCEND_KEY_DOWN,
    DESCEND_KEY_UP
}

public interface JumpStateListener {
    void NotifyOfJumpState(JumpStateEvent jumpStateEvent);
}

public enum JumpStateEvent {
    BEGIN_JUMP,
    BEGIN_FALL
}

public class JumpState : StateMachineListener {
    readonly int maxJumps = 0;

    int jumpsTaken;
    JumpStateType jumpState = JumpStateType.NONE;

    readonly List<JumpStateListener> listeners = new();

    public JumpState(StateMachine stateMachine, int maxJumps = 2) {
        stateMachine.RegisterListener(this);
        this.maxJumps = maxJumps;
    }

    public void RegisterListener(JumpStateListener listener) {
        listeners.Add(listener);
    }
    
    public void OnStateMachineStateChange(StateMachineState from, StateMachineState to) {
        if (to.GetName().ToLower() == "jump") {
            StartJump();
        }
    }
    
    void NotifyAll(JumpStateEvent jumpStateEvent) {
        foreach (var listener in listeners) {
            listener.NotifyOfJumpState(jumpStateEvent);
        }
    }

    public void ReleaseJumpKey() {
        if (jumpState == JumpStateType.ASCEND_KEY_DOWN) {
            jumpState = JumpStateType.ASCEND_KEY_UP;
        } else if (jumpState == JumpStateType.DESCEND_KEY_DOWN) {
            jumpState = JumpStateType.DESCEND_KEY_UP;
        }
    }

    // StartJump is called every time the state machine enters the "jump" state.
    // The larger state machine is purely responsible for keeping track of whether
    // the player is currently jumping and does not keep track of whether they're
    // ascending or falling and how many jumps they've taken
    public void StartJump() {
        if (jumpState != JumpStateType.NONE) {
            Debug.Log($"Warning: Trying to jump while not in JumpStateType.NONE state");
            // return;
        }
        
        jumpsTaken = 1;
        jumpState = JumpStateType.ASCEND_KEY_DOWN;
        NotifyAll(JumpStateEvent.BEGIN_JUMP);
    }
    
    // TryAddJump is safe to call on every frame (i.e. when the "W" key is pressed)
    // and fails if the player is not already jumping
    public bool TryAddJump() {
        if (jumpState != JumpStateType.ASCEND_KEY_UP &&
            jumpState != JumpStateType.DESCEND_KEY_UP) {
            // Debug.Log($"TryAddJump failure {ToString()}");
            return false;
        }

        // Debug.Log($"TryAddJump success {ToString()}");

        if (jumpsTaken >= maxJumps) {
            return false;
        }

        jumpsTaken++;
        jumpState = JumpStateType.ASCEND_KEY_DOWN;
        NotifyAll(JumpStateEvent.BEGIN_JUMP);
        return true;
    }
    
    // If the player begins to fall, their jump state is appropriately
    // updated to reflect it. Whether the player has the key down
    // or up, the state will transition to a descending state
    public void AddFall() {
        if (jumpState == JumpStateType.ASCEND_KEY_DOWN) {
            jumpState = JumpStateType.DESCEND_KEY_DOWN;
            NotifyAll(JumpStateEvent.BEGIN_FALL);
        } else if (jumpState == JumpStateType.ASCEND_KEY_UP) {
            jumpState = JumpStateType.DESCEND_KEY_UP;
            NotifyAll(JumpStateEvent.BEGIN_FALL);
        }
    }

    public void Land() {
        jumpsTaken = 0;
        jumpState = JumpStateType.NONE;
    }
    
    public bool IsFalling() {
        return jumpState == JumpStateType.DESCEND_KEY_UP ||
               jumpState == JumpStateType.DESCEND_KEY_DOWN;
    }

    public bool IsAscending() {
        return jumpState == JumpStateType.ASCEND_KEY_UP ||
               jumpState == JumpStateType.ASCEND_KEY_DOWN;
    }

    public override string ToString() {
        string result = $"{jumpsTaken}/{maxJumps}, state: {jumpState}";
        
        return result;
    }

    public string CurrentState_Debug() {
        return jumpState.ToString();
    }

    public string CurrentJumps_Debug() {
        return $"Jumps: {jumpsTaken}/{maxJumps}";
    }
}