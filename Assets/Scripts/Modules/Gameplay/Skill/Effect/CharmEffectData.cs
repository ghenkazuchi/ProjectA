using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "CharmEffect", menuName = "Effects/Charm Effect")]
public class CharmEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new CharmEffect(this, owner, target, duration);
	}
}
