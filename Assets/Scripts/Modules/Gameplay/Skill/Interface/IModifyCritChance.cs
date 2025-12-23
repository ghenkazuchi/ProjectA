using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IModifyCritChance
{
	public float ModifyCritChance(EntityBase attacker, EntityBase target,float baseCritChance);
}
