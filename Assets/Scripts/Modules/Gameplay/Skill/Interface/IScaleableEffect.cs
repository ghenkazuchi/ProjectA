using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScaleableEffect
{
	public void ApplyGradeTunning(float magnituMulplier,int durationBonus, float procChanceMultiplier, EffectData additionalEffect = null);
}
