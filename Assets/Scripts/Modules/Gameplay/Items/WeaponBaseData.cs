using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon/Weapon Data")]
public class WeaponBaseData : EquipableBaseData
{
	public WeaponType weaponType;
	public WeaponRequirement requirement;
}
