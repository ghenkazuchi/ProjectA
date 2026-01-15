using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseSpellDamageBuffData : EffectBase,IBeforeDealingDamage
{
	public float IncreaseDamagePercentage;
	public IncreaseSpellDamageBuffData(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration, Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1) : base(effectType, effect, name, owner, target, duration, icon, canBeRemoved, stackable, maxStack)
	{
	}

	public IEnumerator OnBeforeDealingDamage(DamageContext ctx)
	{
		if (ctx.Target == null || ctx.Origin != SkillDefinition.Spell)
			yield break;
		var newFinalDamage = ctx.EffectiveDamage + Mathf.CeilToInt(ctx.EffectiveDamage * IncreaseDamagePercentage);
		Debug.Log($"Increase Spell Damage Buff Applied: Original Damage {ctx.EffectiveDamage}, New Damage {newFinalDamage}");
		ctx.EffectiveDamage = newFinalDamage;
		yield break;
	}
	public override string GetExpireMessage()
	{
		return $"{Target.entityData.EntityName}'s magic surge has worn off.";
	}
}
