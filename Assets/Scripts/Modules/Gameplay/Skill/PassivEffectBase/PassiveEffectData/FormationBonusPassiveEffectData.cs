using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FormationBonusPassiveEffectData", menuName = "Skill/PassiveEffect/FormationBonus")]
public class FormationBonusPassiveEffectData : PassiveEffectData
{
	[Header("Front Row Bonuses (x=0) — Melee Stats")]
	public List<FormationStatBonus> frontRowBonuses = new List<FormationStatBonus>();

	[Header("Back Row Bonuses (x=1) — Caster Stats")]
	public List<FormationStatBonus> backRowBonuses = new List<FormationStatBonus>();

	public override PassiveEffectBase CreateRuntimeEffect()
	{
		return new FormationBonusPassiveEffect(frontRowBonuses, backRowBonuses);
	}
}

[System.Serializable]
public struct FormationStatBonus
{
	public Stat stat;
	public ModType modType;
	public float value;
}
