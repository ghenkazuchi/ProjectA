using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FleetingStepEffect : EffectBase, IBeforeTakingDamage
{
	public float evasionPercentage;
	public FleetingStepEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
	}

	public IEnumerator OnBeforeTakingDamage(DamageContext ctx)
	{
		if (ctx.Target == null) yield break;
		var targetPercentageHp = ctx.Target.GetCurrentHP() / (float)ctx.Target.GetFinalStat(Stat.HP);
		if (targetPercentageHp <= evasionPercentage)
		{
			ctx.EffectiveDamage = 0;
			yield return BattleSystem.Instance.ShowDialog($"{ctx.Target.entityData.EntityName} evaded the attack due to {Name}!");
			ctx.Target.RemoveEffect(this);
		}
	}

}
