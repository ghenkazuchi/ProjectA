using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantKillEffect : EffectBase, IOnDealingDamage
{
	public float InstantKillThresholdPercentage { get; set; }
	public InstantKillEffect(EffectData data, EntityBase owner, EntityBase target, int duration, float threshold = 0) : base(data, owner, target, duration)
	{
		InstantKillThresholdPercentage = threshold;
	}

	public override IEnumerator ApplyEffect()
	{
		yield break;
	}

	public override IEnumerator RemoveEffect()
	{	
		yield return null;
	}

	public IEnumerator OnDealingDamage(DamageContext ctx)
	{
		if (ctx.Target == null || ctx.Target.GetCurrentHP() <= 0) yield break;
		
		float targetHPPercentage = (float)ctx.Target.GetCurrentHP() / ctx.Target.GetFinalStat(Stat.HP);
		if (targetHPPercentage <= InstantKillThresholdPercentage)
		{
			// The Collector Execute: Deal overpowering flat true damage
			int executeDamage = 9999;
			ctx.Target.TakeDamage(executeDamage, ctx.Source);
			
			string executionerName = ctx.Source != null ? ctx.Source.entityData.EntityName : "The attacker";
			yield return BattleSystem.Instance.ShowDialog($"The attack was fatal, {ctx.Target.entityData.EntityName} die!");
		}
	}
}
