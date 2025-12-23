using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Add Element Data", menuName = "Effects/Add Element")]
public class AddElementToBattleArtEffectData : EffectData
{
	public Element elementToAdd;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new AddElementToBattleArtEffect(
			EffectType, Effect, Name, owner, target, duration,effectIcon,
			CanBeRemoved, Stackable, MaxStack, elementToAdd
		);
	}
}
