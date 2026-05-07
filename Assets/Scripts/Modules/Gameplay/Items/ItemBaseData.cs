using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Item/Item Data")]
public class ItemBaseData : EquipableBaseData
{
	public bool canBeUpgraded;

	[Header("Grade Bonuses (only for Gold / Diamond)")]
	public GradeTunning gold;
	public GradeTunning diamond;

	[Header("Description at Higher Grade")]
	public string goldDescription;
	public string diamondDescription;

	public GradeTunning GetTuning(ItemGrade g) => g switch
	{
		ItemGrade.Gold    => gold,
		ItemGrade.Diamond => diamond,
		_                 => default
	};

	public Color GetTint(ItemGrade g) => ItemGradeConfig.Instance.GetTint(g);

	public string GetEffectDescriptionByGrade(ItemGrade g) => g switch
	{
		ItemGrade.Gold    => goldDescription,
		ItemGrade.Diamond => diamondDescription,
		_                 => description
	};
}
