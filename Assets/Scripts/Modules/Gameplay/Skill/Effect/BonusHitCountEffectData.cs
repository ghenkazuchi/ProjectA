using UnityEngine;

[CreateAssetMenu(fileName = "Bonus Hit Count Effect", menuName = "Effects/Bonus Hit Count Effect")]
public class BonusHitCountEffectData : EffectData
{
	public int bonusHits = 1;

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new BonusHitCountEffect(this, owner, target, duration);
	}
}
