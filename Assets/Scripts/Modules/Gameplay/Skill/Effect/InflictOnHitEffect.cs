using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflictOnHitEffect : EffectBase, IOnDealingDamage
{
	public EffectData OnHitEffectToApply;
	public InflictOnHitEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
	}

	public override IEnumerator ApplyEffect()
	{
		yield break;
	}

	public IEnumerator OnDealingDamage(DamageContext ctx)
	{
		if (ctx.Target == null) yield break;
		EffectBase effectToApply = OnHitEffectToApply.CreateRuntimeEffect(Owner, ctx.Target, OnHitEffectToApply.MaxDuration);
		yield return ctx.Target.AddEffect(effectToApply);
	}
	public override IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} coating expired! ");
	}
}
