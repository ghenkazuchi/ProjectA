using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParryEffect : EffectBase, IBeforeTakingDamage
{
	public float successfulParryPercentage;

	public ParryEffect(EffectData data, EntityBase owner, EntityBase target, int duration, float successParryPercentage  = 0) : base(data, owner, target, duration)
	{
		successfulParryPercentage = successParryPercentage;
	}

	public override IEnumerator ApplyEffect()
	{
		yield break;
	}

	public IEnumerator OnBeforeTakingDamage(DamageContext ctx)
	{
		if (ctx.Target != Owner) yield break;
		if (ctx.isEffectDamage || ctx.Origin != SkillDefinition.BattleArt) yield break;
		if (ctx.EffectiveDamage <= 0) yield break;
		if (Random.value > successfulParryPercentage) yield break;

		ctx.CancleApply = true;
		ctx.ReflectAmount = ctx.EffectiveDamage;

		yield return BattleSystem.Instance.ShowDialog(
			$"{Owner.entityData.EntityName} parried {ctx.Source.entityData.EntityName}'s attack");
	}

	public override IEnumerator RemoveEffect()
	{
		yield break;
	}
}
