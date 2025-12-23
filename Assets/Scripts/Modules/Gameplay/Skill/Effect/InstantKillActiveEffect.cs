using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantKillActiveEffect : InstantKillEffect
{
	public InstantKillActiveEffect(EffectType effectType, Effect effect, string name, EntityBase owner, EntityBase target, int duration,Sprite icon, bool canBeRemoved = true, bool stackable = false, int maxStack = 1, float instantKillThresholdPercentage = 0) : base(effectType, effect, name, owner, target, duration,icon, canBeRemoved, stackable, maxStack)
	{
		InstantKillThresholdPercentage = instantKillThresholdPercentage;
	}

	public override IEnumerator ApplyEffect()
	{
		int killAmount = Target.GetCurrentHP();
		Target.TakeDamage(killAmount);
		yield return BattleSystem.Instance.ShowDialog($"{Target.entityData.EntityName} was instantly killed !");
		yield return BattleSystem.Instance.ApplyEffectDamage(Target, killAmount, Owner, null);

	}
	public override IEnumerator RemoveEffect()
	{
		yield return null;
	}
}
