using Minifantasy.ForgottenPlains;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnyieldingPassiveEffect : PassiveEffectBase, ILimitedPassiveUse
{
	public bool hasTrigger = false;
	public float RestorationPercentage { get; set; }

	public UnyieldingPassiveEffect(float restorationPercentage)
	{
		RestorationPercentage = restorationPercentage;
	}
	public override IEnumerator ApplyEffect(EntityBase owner, BattleSystem battleSystem, params object[] args)
	{
		if(args.Length >=1 && args[1] is int damage)
		{
			if (!hasTrigger && owner.GetCurrentHP() == 0)
			{
				owner.Heal(Mathf.CeilToInt(owner.MaxHp * RestorationPercentage));
				hasTrigger = true;
				yield return battleSystem.ShowDialog($"{owner.entityData.EntityName} revived");
				battleSystem.UpdateUnitHealth(owner);
			}
		}
	}

	public void ResetUse()
	{
		Debug.Log("Resetting UnyieldingPassiveEffect use");
		hasTrigger = false;
	}
}
