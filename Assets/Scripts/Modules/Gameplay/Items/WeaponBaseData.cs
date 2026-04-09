using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon/Weapon Data")]
public class WeaponBaseData : EquipableBaseData
{
	public WeaponType weaponType;
	public WeaponRequirement requirement;

	[Tooltip("If true, the wielder can single-target protected back-row units (e.g. bows).")]
	public bool canPierceBackRow;

	[Header("Stacking")]
	[Tooltip("If true, picking up a duplicate merges into the existing weapon instead of replacing it.")]
	public bool isStackable = false;
	[Min(1)]
	[Tooltip("Maximum number of stacks this weapon can reach.")]
	public int maxStack = 1;
}
