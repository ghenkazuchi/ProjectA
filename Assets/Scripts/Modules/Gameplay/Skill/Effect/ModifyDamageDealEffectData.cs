using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModifyDamageDeal", menuName = "Effects/ModifyDamageDeal Effect")]
public class ModifyDamageDealEffectData : EffectData
{
	public float increasePercent; 
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration) => new ModifyDamageDealEffect(owner, target, duration,effectIcon, increasePercent);
}
