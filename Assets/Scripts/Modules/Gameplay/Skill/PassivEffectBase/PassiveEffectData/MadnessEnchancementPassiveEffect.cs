using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MadnessEnchancementPassiveEffect : PassiveEffectBase
{
	public float healingPercentage;
	public MadnessEnchancementPassiveEffect(float percentage)
	{
		healingPercentage = percentage;
	}
	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		if(owner == null)
			yield break;
		if(owner.GetCurrentHP()/ (float)owner.MaxHp <= 0.3f)
		{
			int lostHP = owner.MaxHp - owner.GetCurrentHP();
			int healAmount = Mathf.CeilToInt(lostHP * healingPercentage);
			HealingContext ctx = new HealingContext()
			{
				Healer = owner,
				Target = owner,
				BaseHealing = healAmount,
				FinalHealing = healAmount,
			};
			foreach(var effect in owner.GetAllEffect())
			{
				if(effect is IOnHealingReceived hooked)
				{
					hooked.OnHealingReceived(ctx);
				}
			}
			owner.Heal(ctx.FinalHealing);
			if (battleSystem != null)
			{
				battleSystem.UpdateUnitHealth(owner);
				yield return battleSystem.ShowDialog($"{owner.entityData.EntityName}'s Madness triggers! Recovered {ctx.FinalHealing} HP!");
			}
		}
	}
}