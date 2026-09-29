public interface FrameEvent {
	public bool HasTriggered();
	public float GetTriggerTime();
	void Reset();
	void Trigger(float timeMs, Animatable animatable);
	FrameEvent Clone();
}