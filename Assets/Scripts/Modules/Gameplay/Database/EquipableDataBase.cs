using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Create new equipableList Database", menuName = "Data/List Equipable")]
public class EquipableDataBase : ScriptableObject
{
	[SerializeField] EquipableBaseData[] equipableBaseDatas;
}
