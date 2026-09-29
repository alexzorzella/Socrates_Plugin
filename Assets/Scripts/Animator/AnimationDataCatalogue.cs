using System;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationCatalogue {
	static readonly List<CharacterAnimator.Builder> animationCatalogue = new() {
		
	};

	public static CharacterAnimator BuildCharAnimatorForUsing(string characterName, Animatable animatable) {
		CharacterAnimator.Builder animationDataBuilder = 
			Array.Find(animationCatalogue.ToArray(), animation => animation.GetName() == characterName);

		if (animationDataBuilder != null) {
			CharacterAnimator result = animationDataBuilder.Build(animatable);
			return result;
		}

		return null;
	}
}