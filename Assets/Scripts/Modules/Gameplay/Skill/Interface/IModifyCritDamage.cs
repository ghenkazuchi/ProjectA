using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IModifyCritDamage
{
	public float ModifyCritDamage(EntityBase attacker, EntityBase target,float baseMultiplier);
}
