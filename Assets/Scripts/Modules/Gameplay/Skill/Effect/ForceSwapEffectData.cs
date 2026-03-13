using UnityEngine;

[CreateAssetMenu(fileName = "ForceSwapEffect", menuName = "Effects/Force Swap Effect")]
public class ForceSwapEffectData : EffectData
{
	private void OnEnable()
	{
		// Force Swap is always an instant, one-shot effect
		isInstantEffect = true;
	}

	public override EffectBase CreateRuntimeEffect(EntityBase owner, EntityBase target, int duration)
	{
		return new ForceSwapEffect(this, owner, target, duration);
	}
}
