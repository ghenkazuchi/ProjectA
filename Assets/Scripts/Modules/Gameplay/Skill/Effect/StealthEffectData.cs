using UnityEngine;

[CreateAssetMenu(fileName = "Stealth Effect", menuName = "Effects/Stealth Effect")]
public class StealthEffectData : EffectData
{
	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new StealthEffect(this, owner, target, duration);
	}
}
