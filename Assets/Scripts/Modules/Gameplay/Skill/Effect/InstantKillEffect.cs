using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantKillEffect : EffectBase, IOnDealingDamage
{
	public float InstantKillThresholdPercentage { get; set; }
	public InstantKillEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1, float threshold = 0) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
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
			int killAmount = Target.GetCurrentHP();
			Target.TakeDamage(killAmount);
			yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} was instantly killed !");
		}
	}
}
