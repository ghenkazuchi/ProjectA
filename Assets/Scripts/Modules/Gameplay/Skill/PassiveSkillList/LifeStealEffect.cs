using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class LifeStealEffect : PassiveEffectBase
{
	public float LifeStealPercentage { get; set; }
	public LifeStealEffect(float lifeStealPercentage)
	{
		LifeStealPercentage = lifeStealPercentage;
	}
	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		if (args.Length >= 1 && args[1] is int damage)
		{
			int healingAmount = Mathf.CeilToInt(damage * LifeStealPercentage);
			Debug.Log(healingAmount);
			owner.Heal(healingAmount);
			if (healingAmount > 0)
			{
				Debug.Log("LifeStealEffect: " + owner.entityData.EntityName + " healed for " + healingAmount);	
				yield return battleSystem.ShowDialog($"{owner.entityData.EntityName} heal for {healingAmount} due to life steal effect" );
				battleSystem.UpdateUnitHealth(owner);
			}
		}
	}
}
