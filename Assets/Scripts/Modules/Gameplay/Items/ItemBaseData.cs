using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Item/Item Data")]
public class ItemBaseData : EquipableBaseData
{
	public bool canBeUpgraded;
	[Header("Upgrade Settings")]
	public GradeTunning normal;
	public GradeTunning gold;
	public GradeTunning diamond;
	[Header("Icon Tint Per Grade")]
	public Color normalTint = Color.white;
	public Color goldTint = new Color(1f, 0.92f, 0.38f);
	public Color32 diamondTint = new Color32(83, 115, 178, 255);

	[Header("Description at Higher Grade")]
	public string goldDescription;
	public string diamondDescription;
	public GradeTunning GetTuning(ItemGrade g) => g switch
	{
		ItemGrade.Gold => gold,
		ItemGrade.Diamond => diamond,
		_ => normal
	};
	public Color GetTint(ItemGrade g) => g switch
	{
		ItemGrade.Gold => goldTint,
		ItemGrade.Diamond => diamondTint,
		_ => normalTint
	};
	public string GetEffectDescriptionByGrade(ItemGrade g) => g switch
	{
		ItemGrade.Gold => goldDescription,
		ItemGrade.Diamond => diamondDescription,
		_ => description
	};
}
