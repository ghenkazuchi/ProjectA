using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionValueModifiEffect : EffectBase
{
	public ActionValueModifiEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration,Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration,icon, canBeRemoved, stackable, maxStack)
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
