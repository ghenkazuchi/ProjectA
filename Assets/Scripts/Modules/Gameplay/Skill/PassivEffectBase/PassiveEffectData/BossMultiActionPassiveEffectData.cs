using UnityEngine;

[CreateAssetMenu(fileName = "BossMultiActionPassiveEffectData", menuName = "Skill/PassiveEffect/BossMultiAction")]
public class BossMultiActionPassiveEffectData : PassiveEffectData
{
	[Min(1)] public int extraTurnsPerRound = 1;

	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new BossMultiActionPassiveEffect(extraTurnsPerRound);
	}
}
