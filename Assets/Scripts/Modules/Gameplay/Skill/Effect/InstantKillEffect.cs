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
		if (ctx.Target == null) yield break;
		float targetHPPercentage = (float)ctx.Target.GetCurrentHP() / ctx.Target.GetFinalStat(Stat.HP);
		if (targetHPPercentage <= InstantKillThresholdPercentage)
		{
			int killAmount = ctx.Target.GetCurrentHP();
			ctx.Target.TakeDamage(killAmount);
			yield return BattleSystem.Instance.ShowDialog($"{ctx.Target.entityData.EntityName} was instantly killed !");
		}
	}
}
