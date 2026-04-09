using UnityEngine;

[CreateAssetMenu(fileName = "Extra Turn Effect Data", menuName = "Effects/Extra Turn Effect Data")]
public class ExtraTurnEffectData : EffectData
{
	private void OnEnable()
	{
		isInstantEffect = true;
	}

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ExtraTurnEffect(this, owner, target, duration);
	}
}
