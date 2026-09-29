using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class CharacterAnimator : StateMachineListener {
	// Everything is immutable except for the current animation and the current animation time
	readonly string characterName;
	readonly RuntimeAnimatorController runtimeAnimatorController;
	readonly List<AnimationData> animations;
	readonly Animatable animatable;
	
	AnimationData currentAnimation;
	float currentAnimationTimeMs;

	GameManager manager;
	float timeAnimationStarted = 0;
	
	public void SetAnimatorSpeed(float speed) { animatable.GetAnimator().speed = speed; }

	/// <summary>
	/// Only the builder calls this constructor
	/// </summary>
	/// <param name="characterName"></param>
	/// <param name="runtimeAnimatorController"></param>
	/// <param name="animations"></param>
	/// <param name="animatable"></param>
	/// <param name="stalingCache"></param>
	CharacterAnimator(
		string characterName, 
		RuntimeAnimatorController runtimeAnimatorController,
		List<AnimationData> animations,
		Animatable animatable) {
		manager = GameManager.Instance();
		
		this.characterName = characterName;
		this.runtimeAnimatorController = runtimeAnimatorController;
		this.animations = animations;

		this.animatable = animatable;
		
		ResetAllAnimations();
	}
	
	/// <summary>
	/// Important: this function must be externally called every frame!
	///  
	/// Advances the current animation time and calls Update on the current animation
	/// which triggers any animation events that should occur or should have occured in the past.
	/// If the current animation is completed, then it is reset with looping allowed so that
	/// animations that loop can have their events trigger again
	/// </summary>
	public void UpdateAnimations() {
		// if (paused_condition) {
		// 	return;
		// }
		
		if (currentAnimation != null) {
			// currentAnimationTimeMs += Time.deltaTime * 1000F;
			currentAnimationTimeMs = (manager.TimeSinceLevelLoad() - timeAnimationStarted) * 1000F;

			bool animationComplete = currentAnimation.Update(currentAnimationTimeMs, animatable);
			
			if (animationComplete) {
				ResetCurrentAnimationAllowingLooping();
			}
		}
	}
	
	/// <summary>
	/// Force plays the animation with the passed name if it's found in the list of animations
	/// </summary>
	/// <param name="animationName"></param>
	void ForcePlay(string animationName) {
		CancelCurrentAnimationHard();
		ResetAllAnimations();

		// Try to find the animation data
		AnimationData animation = Array.Find(animations.ToArray(), animation => animation.GetName() == animationName);
		
		// If it's not null, then it should be cached as the current animation
		if (animation != null) {
			currentAnimation = animation;
		} else { // If the animation data is null, it's going to have to be created on the spot
			AnimationClip associatedClip = Array.Find(runtimeAnimatorController.animationClips.ToArray(),
				clip => clip.name == animationName);
		
			if (associatedClip != null) {
				AnimationData newAnimation = new AnimationData.Builder(animationName).Build(associatedClip);
				animations.Add(newAnimation);
				
				currentAnimation = newAnimation;
			}
		}
		
		// Regardless of whether there were events or not, it is played through the actual Unity animation system
		animatable.GetAnimator().Play(animationName);
	}
	
	/// <summary>
	/// Disables all the hitboxes currently active, and then
	/// resets the current animation if there is one
	/// </summary>
	public void CancelCurrentAnimationHard() {
		if (currentAnimation != null) {
			ResetCurrentAnimationHard();
		}
	}
	
	/// <summary>
	/// Zeroes the current animation time, resets the current animation,
	/// and only sets the current animation to null if it doesn't loop
	/// </summary>
	void ResetCurrentAnimationAllowingLooping() {
		timeAnimationStarted = manager.TimeSinceLevelLoad();
		currentAnimationTimeMs = 0;
		currentAnimation.Reset();
		
		if (!currentAnimation.Loops()) {
			currentAnimation = null;
		}
	}

	/// <summary>
	/// Zeroes the current animation time, resets the current animation,
	/// and always sets the current animation to null
	/// </summary>
	void ResetCurrentAnimationHard() {
		timeAnimationStarted = manager.TimeSinceLevelLoad();
		currentAnimationTimeMs = 0;
		currentAnimation.Reset();

		currentAnimation = null;
	}
	
	/// <summary>
	/// Resets every animation in the list
	/// </summary>
	void ResetAllAnimations() {
		foreach (var animation in animations) {
			animation.Reset();
		}
	}
	
	/// <summary>
	/// Returns the character's runtimeAnimatorController
	/// </summary>
	/// <returns></returns>
	public RuntimeAnimatorController GetRuntimeAnimatorController() {
		return runtimeAnimatorController;
	}

	/// <summary>
	/// Returns true if the current animation is null or if the current
	/// animation is cancelable
	/// </summary>
	/// <returns></returns>
	public bool OnStandby() {
		return currentAnimation?.IsCancelable() ?? true;
	}

	/// <summary>
	/// Called whenever the state machine this is registered to transitions from
	/// a state to another
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	public void OnStateMachineStateChange(StateMachineState from, StateMachineState to) {
		string newStateName = to.GetName();
		ForcePlay(newStateName);
	}

	/// <summary>
	/// The CharacterAnimator's builder is fully responsible for creating new instances of the
	/// class. To successfully build a new CharacterAnimator, the builder must be provided an
	/// instance of AlexAnimationFuncs so that it can cache it for later use when Update(...)
	/// is called on the current animation 
	/// </summary>
	public class Builder {
		string characterName;
		RuntimeAnimatorController runtimeAnimatorController;
		List<AnimationData.Builder> animationBuilders = new();
		
		public Builder WithName(string characterName) {
			this.characterName = characterName;
			return this;
		}
		
		public string GetName() {
			return characterName;
		}

		public Builder WithRuntimeAnimControllerCalled(string animatorControllerName) {
			runtimeAnimatorController = ResourceLoader.i.LoadAnimatorController(animatorControllerName);
			return this;
		}
		
		public Builder WithAnimation(AnimationData.Builder animationDataBuilder) {
			animationBuilders.Add(animationDataBuilder);
			
			return this;
		}

		public CharacterAnimator Build(Animatable animatable) {
			if (runtimeAnimatorController == null) {
				throw new NullReferenceException($"Runtime animator for {characterName}'s CharacterAnimator is null.");
			}

			if (animationBuilders.Count <= 0) {
				Debug.LogWarning($"Did you intend to create {characterName} as a CharacterAnimator shell?");
			}
			
			List<AnimationData> animations = new();

			foreach (var builder in animationBuilders) {
				string clipName = builder.GetName();
				
				AnimationClip associatedClip = Array.Find(runtimeAnimatorController.animationClips.ToArray(),
					clip => clip.name == clipName);

				if (associatedClip != null) {
					animations.Add(builder.Build(associatedClip));
				} else {
					throw new NullReferenceException($"Animation clip '{clipName}' not found.");
				}
			}

			CharacterAnimator result = new(characterName, runtimeAnimatorController, animations, animatable);
			
			return result;
		}
	}
}
