using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleArtBanEffect", menuName = "Effect/Battle Art Ban")]
public class BattleArtBanEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new BattleArtBanEffect(this, owner, target, duration);
	}
}
