public interface StateMachineListener {
    void OnStateMachineStateChange(StateMachineState from, StateMachineState to);
}