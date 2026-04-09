using UnityEngine;

[CreateAssetMenu(fileName = "Critical Modify Effect Data", menuName = "Effects/Critical Modify Effect Data")]
public class CriticalModifyEffectData : EffectData
{
	[Header("Critical Bonuses")]
	[Tooltip("Flat bonus to Critical Chance (0.10 = +10%)")]
	public float bonusCritChance;
	[Tooltip("Flat bonus to Critical Multiplier (0.50 = +50% damage)")]
	public float bonusCritMultiplier;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new CriticalModifyEffect(this, owner, target, duration, bonusCritChance, bonusCritMultiplier);
	}
}
