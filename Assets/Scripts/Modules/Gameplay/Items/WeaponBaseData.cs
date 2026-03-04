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
}
