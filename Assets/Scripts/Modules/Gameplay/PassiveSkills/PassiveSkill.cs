using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PassiveSkill
{
	[field: SerializeField]
	public PassiveSkillData PassiveSkillData { get; set; }
	public int CurrentSkillCoolDown { get; set; }

	public List<PassiveEffectBase> RuntimeEffects { get; private set; }

	public PassiveSkill(PassiveSkillData passiveSkillData, int currentSkillCoolDown)
	{
		PassiveSkillData = passiveSkillData;
		CurrentSkillCoolDown = currentSkillCoolDown;
		InitializeRuntimeEffects();
	}
	public bool IsOnSkillCoolDown()
	{
		return CurrentSkillCoolDown > 0;
	}
	public void StartCoolDown()
	{
		CurrentSkillCoolDown = PassiveSkillData.cooldownTurns;
	}

	public void ReduceCoolDown()
	{
		if (CurrentSkillCoolDown > 0)
			CurrentSkillCoolDown--;
	}
	public void InitializeRuntimeEffects()
	{
		RuntimeEffects = new List<PassiveEffectBase>();

		if (PassiveSkillData.effects == null) return;

		foreach (var effectData in PassiveSkillData.effects)
		{
			if (effectData != null)
			{
				RuntimeEffects.Add(effectData.CreateRuntimeEffect());
			}
			else
			{
				Debug.LogWarning($"A null PassiveEffectData was found in {PassiveSkillData.name}. Skipping this effect.");
			}
		}
	}

}