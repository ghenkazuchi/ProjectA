using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunTimeEntityData : BaseEntityData
{
	public void SetTraits(Dictionary<Trait, int> traitValues)
	{
		foreach (var kvp in traitValues)
		{
			if (BaseTraits.ContainsKey(kvp.Key))
			{
				BaseTraits[kvp.Key] = kvp.Value;
			}
			else
			{
				BaseTraits.Add(kvp.Key, kvp.Value);
			}
		}
	}
	public void CloneFrom(BaseEntityData src)
	{
		EntityName = src.EntityName;
		EntitySprite = src.EntitySprite;
		EntityPortrait = src.EntityPortrait;
		EntityElement = src.EntityElement;
		exclusiveActiveSkill = src.ExclusiveActiveSkill;
		exclusivePassiveSkill = src.ExclusivePassiveSkill;
	}
}
