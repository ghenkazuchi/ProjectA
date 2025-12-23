using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModifyDamageTake", menuName = "Effects/ModifyDamageTaken Effect")]
public class ModifyDamageTakenEffectData : EffectData
{
	public float IncreasePercentage;
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration) => new ModiftyDamageTaken(owner, target, duration,effectIcon, IncreasePercentage);
}
