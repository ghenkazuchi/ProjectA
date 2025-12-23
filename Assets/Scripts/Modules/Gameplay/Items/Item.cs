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
