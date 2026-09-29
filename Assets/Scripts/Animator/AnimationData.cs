using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public class AnimationData {
	readonly string animationName;
	readonly List<FrameEvent> frameEvents;
	readonly float animationLength;
	readonly bool loops;
	readonly bool cancelable;
	
	// For cloning
	AnimationData(string animationName, float animationLength, bool loops, bool cancelable, List<FrameEvent> frameEvents) {
		this.animationName = animationName;
		this.animationLength = animationLength;
		this.frameEvents = frameEvents.ToList();
		this.loops = loops;
		this.cancelable = cancelable;
	}
	
	public string GetName() {
		return animationName;
	}
	
	public bool Loops() {
		return loops;
	}

	public bool IsCancelable() {
		return cancelable;
	}
	
	public bool Update(float timeMs, Animatable animatable, float speedMultiplier = 1) {
		foreach (var frameEvent in frameEvents) {
			if (frameEvent.GetTriggerTime() <= timeMs * speedMultiplier && !frameEvent.HasTriggered()) {
				frameEvent.Trigger(timeMs, animatable);
			}
		}

		return AnimationComplete(timeMs, speedMultiplier);
	}

	public bool AnimationComplete(float timeMs, float speedMultiplier) {
		if (timeMs * speedMultiplier < animationLength) {
			return false;
		}

		foreach (var animationEvent in frameEvents) {
			if (!animationEvent.HasTriggered()) {
				return false;
			}
		}
		
		return true;
	}

	public void Reset() {
		foreach (var frameEvent in frameEvents) {
			frameEvent.Reset();
		}
	}

	public override string ToString() {
		return $"AnimData: {animationName}, {animationLength} ms, Loops: {loops}, Cancelable: {cancelable}";
	}

	public class Builder {
		readonly string animationName;
		readonly List<FrameEvent> frameEvents = new();
		float animationLength = -1;
		bool loops = false;
		bool cancelable;

		public Builder(string animationName) {
			this.animationName = animationName;
		}

		public string GetName() {
			return animationName;
		}
		
		public Builder WithEvent(FrameEvent newFrameEvent) {
			frameEvents.Add(newFrameEvent);
			return this;
		}

		public bool HasEvent<T>() where T : FrameEvent {
			foreach(var frameEvent in frameEvents) {
				if(frameEvent is T) {
					return true;
				}
			}

			return false;
		}

		public Builder IsCancelable() {
			cancelable = true;
			return this;
		}

		public Builder LastsForever() {
			animationLength = float.PositiveInfinity;
			return this;
		}
		
		public AnimationData Build(AnimationClip animationClip) {
			if (animationClip != null) {
				loops = animationClip.isLooping;

				if (animationLength < 0) {
					animationLength = animationClip.length * 1000; // Convert to milliseconds
				}
			}

			List<FrameEvent> clonedEvents = new();
			
			foreach (var frameEvent in frameEvents) {
				FrameEvent clonedEvent = frameEvent.Clone();
				clonedEvents.Add(clonedEvent);
			}
			
			return new AnimationData(animationName, animationLength, loops, cancelable, clonedEvents);
		}
	}
}