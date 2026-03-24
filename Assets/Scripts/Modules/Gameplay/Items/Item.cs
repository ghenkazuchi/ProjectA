using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Item
{
	[field: SerializeField]
	public ItemBaseData itemBaseData;
	public ItemGrade currentItemGrade;
	public Dictionary<string, EffectUsageTracker> effectUsageCounter = new Dictionary<string, EffectUsageTracker>();
	public Item(ItemBaseData itemBaseData, ItemGrade currentItemGrade)
	{
		this.itemBaseData = itemBaseData;
		this.currentItemGrade = currentItemGrade;
		foreach (var effect in itemBaseData.effectData)
		{
			effectUsageCounter.Add(effect.effect.Name, new EffectUsageTracker(itemBaseData.maxUsePerBattle,itemBaseData.maxUsePerLifeCyle));
		}
	}
	public EffectUsageTracker GetEffectTracker(string effectName)
	{
		if (effectUsageCounter == null || effectUsageCounter.Count == 0)
		{
			effectUsageCounter = new Dictionary<string, EffectUsageTracker>();
			if (itemBaseData != null && itemBaseData.effectData != null)
			{
				foreach (var effect in itemBaseData.effectData)
				{
					if (effect != null && effect.effect != null && !effectUsageCounter.ContainsKey(effect.effect.Name))
					{
						effectUsageCounter.Add(effect.effect.Name, new EffectUsageTracker(itemBaseData.maxUsePerBattle, itemBaseData.maxUsePerLifeCyle));
					}
				}
			}
		}

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
}
