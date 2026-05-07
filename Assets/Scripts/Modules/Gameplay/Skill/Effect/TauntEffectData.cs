using UnityEngine;

[CreateAssetMenu(fileName = "Taunt Effect", menuName = "Effects/Taunt Effect")]
public class TauntEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new TauntEffect(this, owner, target, duration);
	}
}
