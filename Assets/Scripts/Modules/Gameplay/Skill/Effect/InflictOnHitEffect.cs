using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflictOnHitEffect : EffectBase, IOnDealingDamage
{
	public EffectData OnHitEffectToApply;
	public InflictOnHitEffect(EffectData data, EntityBase owner, EntityBase target, int duration) : base(data, owner, target, duration)
	{
	}

	public override IEnumerator ApplyEffect()
	{
		yield break;
	}

	public IEnumerator OnDealingDamage(DamageContext ctx)
	{
		if (ctx.Target == null || ctx.Target == Owner) yield break;
		Debug.Log("InflictOnHitEffect triggered");
		Debug.Log($"{Target.entityData.EntityName} {this.Name}");	
		EffectBase effectToApply = OnHitEffectToApply.CreateRuntimeEffect(Owner, ctx.Target, OnHitEffectToApply.MaxDuration);
		yield return ctx.Target.AddEffect(effectToApply);
	}
	public override IEnumerator RemoveEffect()
	{
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} coating expired! ");
	}
}
