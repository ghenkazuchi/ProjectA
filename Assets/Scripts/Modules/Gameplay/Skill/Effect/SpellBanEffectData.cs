using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpellBanEffect", menuName = "Effect/SpellBan")]
public class SpellBanEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new SpellBanEffect(EffectType, Effect, Name, owner, target, duration,effectIcon, CanBeRemoved, Stackable, MaxStack);
	}
}
