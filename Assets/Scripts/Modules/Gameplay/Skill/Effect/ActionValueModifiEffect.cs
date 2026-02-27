using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionValueModifiEffect : EffectBase
{
	public ActionValueModifiEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}
	public float ActionAdvantageValue { get; set; }
	public override IEnumerator ApplyEffect()
	{
		yield return base.ApplyEffect();
		//TimelineManager.Instance.AdvanceFowardEntity(Target, ActionAdvantageValue);
		TimelineManager.Instance.PushToFront(Target);
		BattleSystem.Instance.UpdateTimelineUI();
		Target.RemoveEffect(this);
	}

	public override IEnumerator RemoveEffect()
	{
		yield break;
	}
}
