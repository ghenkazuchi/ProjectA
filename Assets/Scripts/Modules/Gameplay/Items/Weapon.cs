using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
	[field: SerializeField]
	public WeaponBaseData WeaponBaseData;
	[field: SerializeField]
	public int CurrentStack { get; set; } = 1;
	[field: SerializeField]
	public Dictionary<string, EffectUsageTracker> effectUsageCounter = new Dictionary<string, EffectUsageTracker>();
	public readonly Dictionary<object, HashSet<string>> sourceToEffectNames = new Dictionary<object, HashSet<string>>();
	public Weapon(WeaponBaseData weaponBaseData)
	{
		WeaponBaseData = weaponBaseData;
		CurrentStack = 1;
		foreach(var effect in weaponBaseData.effectData)
		{
			Debug.Log(effect.effect.Name);
			effectUsageCounter.Add(effect.effect.Name, new EffectUsageTracker(WeaponBaseData.maxUsePerBattle, WeaponBaseData.maxUsePerLifeCyle));
		}
	}

	public bool CanStack => WeaponBaseData != null && WeaponBaseData.isStackable && CurrentStack < WeaponBaseData.maxStack;

	public bool TryAddStack()
	{
		if (!CanStack) return false;
		CurrentStack++;
		Debug.Log($"[Weapon] {WeaponBaseData.itemName} stacked to {CurrentStack}/{WeaponBaseData.maxStack}");
		return true;
	}
	public EffectUsageTracker GetEffectTracker(string effectName)
	{
		if (effectUsageCounter.TryGetValue(effectName, out var tracker))
		{
			return tracker;
		}
		return null;
	}
	public void ResetBattleUsage()
	{
		foreach (var tracker in effectUsageCounter.Values)
		{
			tracker.ResetBattleUsage();
		}
	}

	public void RegisterEffectBinding(object source, List<EquipEffectBinding> bindings) 
	{
		if(!sourceToEffectNames.ContainsKey(source))
		{
			sourceToEffectNames[source] = new HashSet<string>();
		}

	}

}
