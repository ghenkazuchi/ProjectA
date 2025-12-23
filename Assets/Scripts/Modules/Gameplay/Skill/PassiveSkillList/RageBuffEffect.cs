using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RageBuffEffect : PassiveEffectBase
{
	public EffectData RageEffect { get; set; }

	public RageBuffEffect(EffectData rageEffect)
	{
		RageEffect = rageEffect;
	}

	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{

		var effect =  RageEffect.CreateRuntimeEffect(owner, owner, RageEffect.MaxDuration);
		yield return owner.AddEffect(effect);
	}
}
